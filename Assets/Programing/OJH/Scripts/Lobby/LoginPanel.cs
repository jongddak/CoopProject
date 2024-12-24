using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : UIBInder
{
    private string _email;

    private string _password;

    private StringBuilder _sb = new StringBuilder();

    [SerializeField] private SceneChanger _sceneChanger;

    private void Awake()
    {
        BindAll();
    }
    void Start()
    {
        _sceneChanger.CanChangeSceen = false;
        GetUI<Button>("LoginButton").onClick.AddListener(Login);
        GetUI<Button>("LoginExitButton").onClick.AddListener(_sceneChanger.QuitGame);
    }


    private void Login()
    {
        Debug.Log("로그인 성공");

        _email = GetUI<TMP_InputField>("LoginEmailInputField").text;

        _password = GetUI<TMP_InputField>("LoginPwInputField").text;

        BackendManager.Auth.SignInWithEmailAndPasswordAsync(_email, _password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Exception exception = task.Exception.InnerException;

                FirebaseException firebaseException = exception as FirebaseException;

                if (firebaseException != null)
                {
                    AuthError errorCode = (AuthError)firebaseException.ErrorCode;
                    Debug.Log(errorCode);

                    switch (errorCode)
                    {
                        case AuthError.InvalidEmail:
                            SetTrueWarningPanel("InvalidEmail");
                            break;
                        case AuthError.UserNotFound:
                            SetTrueWarningPanel("UserNotFound");
                            break;
                        case AuthError.MissingEmail:
                            SetTrueWarningPanel("MissingEmail");
                            break;
                        case AuthError.MissingPassword:
                            SetTrueWarningPanel("MissingPassword");
                            break;
                        case AuthError.WeakPassword:
                            SetTrueWarningPanel("WeakPassword");
                            break;
                        case AuthError.WrongPassword:
                            SetTrueWarningPanel("WrongPassword");
                            break;
                        default:
                            SetTrueWarningPanel("Unknown Error. Try Again.");
                            break;
                    }
                }
                else
                {
                    Debug.LogError("FirebaseException이 아닌 다른 예외가 발생했습니다: " + exception?.ToString());
                }

                return;
            }

            AuthResult result = task.Result;
            Debug.Log($"User signed in successfully: {result.User.DisplayName} ({result.User.UserId})");
            CheckUserInfo();
        });
    }

    public void CheckUserInfo()
    {
        Debug.Log("첫 로그인!!!!");
        FirebaseUser user = BackendManager.Auth.CurrentUser;
        if (user == null)
        {
            return;
        }

        // 첫 로그인 시 이메일 인증 및 닉네임 설정 진행
        if (user.IsEmailVerified == false)
        {
            //TODO : 이메일 인증 진행
            GetUI<VerifyPanel>("VerifyPanel").namePanel = GetUI("NamePanel").gameObject;
            GetUI<VerifyPanel>("VerifyPanel").gameObject.SetActive(true);

        }
        else if (user.DisplayName == "")  // 인증만하고 이름 설정안한 경우도 고려.
        {
            //TODO : 닉네임 설정 진행
            GetUI("NamePanel").SetActive(true);
        }
        else
        {
            //TODO :  로비씬으로 비동기 씬 전환 
            _sceneChanger.ChangeScene("LobbyOJH");

            //ToDo: DB에서 PlayerData 불러오기 
            GetPlayerData();

            // 초기화 끝나고 나면 씬change진행.
            _sceneChanger.CanChangeSceen = true;
        }
    }

    private void GetPlayerData()
    {
        FirebaseUser user = BackendManager.Auth.CurrentUser;

        DatabaseReference root = BackendManager.Database.RootReference.Child("UserData").Child(user.UserId);

        root.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("GetValueAsync encountered an error: " + task.Exception);
                return;
            }

            DataSnapshot snapShot = task.Result;

            PlayerDataManager.Instance.PlayerData.PlayerName = snapShot.Child("_playerName").Value.ToString();
            PlayerDataManager.Instance.PlayerData.PlayerId = snapShot.Child("_playerId").Value.ToString();

            var moneyChildren = snapShot.Child("_money").Children.ToList();

            Debug.Log(moneyChildren.Count);
            for (int i = 0; i < moneyChildren.Count; i++)
            {
                Debug.Log(int.Parse(moneyChildren[i].Value.ToString()));
                //PlayerDataManager.Instance.PlayerData.Money[i] = int.Parse(moneyChildren[i].Value.ToString());
                PlayerDataManager.Instance.PlayerData.Money[i] = TypeCastManager.Instance.TryParseInt(moneyChildren[i].Value.ToString());
            }

            var unitDataChildren = snapShot.Child("_unitDatas");

            foreach (var unitChild in unitDataChildren.Children)
            {
                PlayerUnitData unitData = new PlayerUnitData
                {
                    Name = unitChild.Child("_name").Value.ToString(),
                    UnitLevel = int.Parse(unitChild.Child("_unitLevel").Value.ToString()),
                    Type = unitChild.Child("_type").Value.ToString(),
                    ElementName = unitChild.Child("_elementName").Value.ToString(),
                    Hp = TypeCastManager.Instance.TryParseInt(unitChild.Child("_hp").Value.ToString()),
                    Atk = TypeCastManager.Instance.TryParseInt(unitChild.Child("_atk").Value.ToString()),
                    Def = TypeCastManager.Instance.TryParseInt(unitChild.Child("_def").Value.ToString()),
                    Grid = unitChild.Child("_grid").Value.ToString(),
                    StatId = TypeCastManager.Instance.TryParseInt(unitChild.Child("_statId").Value.ToString()),
                    PercentIncrease = TypeCastManager.Instance.TryParseInt(unitChild.Child("_percentIncrease").Value.ToString())
                };
                PlayerDataManager.Instance.PlayerData.UnitDatas.Add(unitData);
            }

        });
    }

    private void SetTrueWarningPanel(string textName)
    {
        GetUI<Image>("LoginWarningPanel").gameObject.SetActive(true);
        _sb.Clear();
        _sb.Append(textName);
        GetUI<TextMeshProUGUI>("WarningText").SetText(_sb);
    }
}
