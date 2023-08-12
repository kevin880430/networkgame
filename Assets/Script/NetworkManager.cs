using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;

namespace Photon.Pun.Demo.Asteroids                                                      //Phonton namespceだけ使える関数、変数などを継承する
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        //Photonデータ
        Dictionary<string, RoomInfo> cachedRoomList;                                        //部屋リスト
       
        Dictionary<string, GameObject> roomListEntries;                                     //部屋リストデータ
        
        Dictionary<string, GameObject> playerListEntries;                                   //プレイヤーデータ

        //0.InputPlayerName
        [Header("プレイヤー名前入力枠")]
        public InputField PlayerNameInput;
        [Header("通信状態テキスト")]
        public Text ConnectInfoText;
        [Header("ログインページ")]
        public GameObject LoginPanel;
        [Header("名前入力してないUI")]
        public GameObject NameErrorPage;
        [Header("部屋を作る/選ぶUI")]
        public GameObject OptionPage;

        //2.部屋を選ぶ
        [Header("部屋を作るUI")]
        public GameObject CreateRoomPanel;
        [Header("部屋の名前入力枠")]
        public InputField RoomNameInput;

        //4.部屋リスト
        [Header("全ての部屋リストページ")]
        public GameObject RoomListPage;
        [Header("部屋リストのRoomListContent")]
        public GameObject RoomListContent;
        [Header("生成される部屋リストオブジェクト")]
        public GameObject RoomListPrefab;
        void Start()
        {
            PhotonNetwork.AutomaticallySyncScene = true;                                    //同じ部屋にいるプレイヤーは同じSceneに入るオブジェクトを同期する
            
            cachedRoomList = new Dictionary<string, RoomInfo>();                            //部屋リスト初期化
            
            roomListEntries = new Dictionary<string, GameObject>();                         //部屋データ初期化
        }

        // Update is called once per frame
        void Update()
        {
            ConnectInfoText.text = PhotonNetwork.NetworkClientState.ToString();             //通信状態を提示
        }
        #region 0.プレイヤー名前入力、ログイン
        //ログインボタンを押すと
        public void LoginBtn() 
        {
            if(PlayerNameInput.text=="")
            {
                NameErrorPage.SetActive(true);                                              //エラー提示
            }
            else
            {
                PhotonNetwork.LocalPlayer.NickName = PlayerNameInput.text;                  //入力された名前を一時的にニックネームに保存する

                PhotonNetwork.ConnectUsingSettings();                                       //Photon環境設定を行う

                LoginPanel.SetActive(false);                                                //ログイン画面を閉じる
            }
        }
        //通信状態を更新
        public override void OnConnectedToMaster()
        {
            OptionPage.SetActive(true);                                                     //ログインした後直接部屋選ぶページを開く
        }

        #endregion
        #region 2.部屋を作る
        public void CreateRoom() 
        {
            CreateRoomPanel.SetActive(true);                                                //開く3.部屋を作るUI

            OptionPage.SetActive(false);                                                    //閉じる2.部屋を作る/選ぶUI

            RoomNameInput.text = "";                                                        //部屋の名前入力枠初期化
        }
        //プレイヤーが3.部屋を作るUIで部屋の名前を入力
        public void CreateRoomConBtn() 
        {
            string roomName = RoomNameInput.text;

            roomName = (roomName.Equals(string.Empty)) ? "Room" + Random.Range(1000, 10000) : roomName; //プレイヤーが部屋の名前入力してない場合ランダムで名前を生成

            RoomOptions options = new RoomOptions { MaxPlayers = 2, PlayerTtl = 10000 };                //プレイヤーの最大人数、プレイヤーがが落ちた時10000秒以内再接続出来る

            PhotonNetwork.CreateRoom(roomName, options, null);                                          //部屋を作る
        }
        //部屋を作れなかった時2.部屋を作る/選ぶUIに戻る
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            CreateRoomPanel.SetActive(false);                                                           //閉じる3.部屋を作るUI

            OptionPage.SetActive(true);                                                                 //開く2.部屋を作る/選ぶUI
        }
        //2.部屋を作る/選ぶUIから0.ログインページに戻るとき
        public void BackLoginPanel() 
        {
            PhotonNetwork.Disconnect();                                                                 //一回接続を切断

            Application.LoadLevel(Application.loadedLevel);                                             //リロードする
        }
        #endregion
        #region 4.部屋リストを確認する
        //2.で部屋を探すを押すと4.部屋リストUIを開く
        public void RoomListBtn() 
        {
            if (!PhotonNetwork.InLobby)                                                     //Lobbyにいない時
            {
                PhotonNetwork.JoinLobby();                                                  //Lobbyに入る
            }
            RoomListPage.SetActive(true);                                                   //開く4.部屋リスト

            OptionPage.SetActive(false);                                                    //閉じる2.部屋を作る/選ぶUI
        }
        //部屋リストを初期化/更新
        public override void OnJoinedLobby()
        {
           cachedRoomList.Clear();                                                     //部屋リストを削除する

           ClearRoomListView();                                                        //部屋リストのPrefabを削除する
        }
        //部屋リストデータを削除する
            void ClearRoomListView() 
        {
            foreach (GameObject entry in roomListEntries.Values)                        //roomListEntriesのデータを取得する
            {
                Destroy(entry.gameObject);                                              //部屋リストのPrefabを削除する
            }

            roomListEntries.Clear();                                                    //roomListEntriesのデータを更新
        }
        //部屋リストアクセスできない時2.部屋を作る/選ぶUIに戻る
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            OptionPage.SetActive(true);
            RoomListPage.SetActive(false);
        }
        //部屋リストデータを更新する
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            ClearRoomListView();                                                        //部屋リストのPrefabを削除する

            UpdateCachedRoomList(roomList);                                             //部屋データを更新する

            UpdateRoomListView();                                                       //RoomListContentのPrefabを更新する
        }
        void UpdateCachedRoomList(List<RoomInfo> roomList)
        {
            foreach (RoomInfo info in roomList)                                         //もしこの部屋の状態は閉じた、見えない、消されたリストから部屋Prefabを削除
            {
                if (!info.IsOpen||!info.IsVisible || info.RemovedFromList) 
                {
                    if (cachedRoomList.ContainsKey(info.Name))
                    {
                        cachedRoomList.Remove(info.Name);
                    }
                    continue;
                }
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList[info.Name] = info;
                }
                else
                {
                    cachedRoomList.Add(info.Name, info);
                }
            }
        }
        private void UpdateRoomListView()
        {
            foreach(RoomInfo info in cachedRoomList.Values)
            {
                GameObject entry = Instantiate(RoomListPrefab);                                                     //部屋Prefabを生成

                entry.transform.SetParent(RoomListContent.transform);                                               //生成された部屋をRoomListContentの子供にする

                entry.transform.localScale = Vector3.one;                                                           //生成された部屋のサイズを初期化

                entry.GetComponent<RoomListEntry>().Initialize(info.Name, (byte)info.PlayerCount, (byte)info.MaxPlayers); //部屋データを初期化

                roomListEntries.Add(info.Name, entry);                                                              //部屋データを全ての部屋に代入
            }
        }

        #endregion
        #region 4.部屋から部屋を作る/選ぶ画面
        public void BackBtn()
        {
            if (PhotonNetwork.InLobby)                                                      //もしLobbyにいる
            {
                PhotonNetwork.LeaveLobby();                                                 //Lobbyから出る
            }
            RoomListPage.SetActive(false);                                                  //閉じる4.部屋リストUI

            OptionPage.SetActive(true);                                                     //開く2.部屋を作る/選ぶUI
        }

        
        #endregion
    }
}
