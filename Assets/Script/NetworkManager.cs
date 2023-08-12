using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;

namespace Photon.Pun.Demo.Asteroids                                                      //Phonton namespce�����g����֐��A�ϐ��Ȃǂ��p������
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        //Photon�f�[�^
        Dictionary<string, RoomInfo> cachedRoomList;                                        //�������X�g
       
        Dictionary<string, GameObject> roomListEntries;                                     //�������X�g�f�[�^
        
        Dictionary<string, GameObject> playerListEntries;                                   //�v���C���[�f�[�^

        //0.InputPlayerName
        [Header("�v���C���[���O���͘g")]
        public InputField PlayerNameInput;
        [Header("�ʐM��ԃe�L�X�g")]
        public Text ConnectInfoText;
        [Header("���O�C���y�[�W")]
        public GameObject LoginPanel;
        [Header("���O���͂��ĂȂ�UI")]
        public GameObject NameErrorPage;
        [Header("���������/�I��UI")]
        public GameObject OptionPage;

        //2.������I��
        [Header("���������UI")]
        public GameObject CreateRoomPanel;
        [Header("�����̖��O���͘g")]
        public InputField RoomNameInput;

        //4.�������X�g
        [Header("�S�Ă̕������X�g�y�[�W")]
        public GameObject RoomListPage;
        [Header("�������X�g��RoomListContent")]
        public GameObject RoomListContent;
        [Header("��������镔�����X�g�I�u�W�F�N�g")]
        public GameObject RoomListPrefab;
        void Start()
        {
            PhotonNetwork.AutomaticallySyncScene = true;                                    //���������ɂ���v���C���[�͓���Scene�ɓ���I�u�W�F�N�g�𓯊�����
            
            cachedRoomList = new Dictionary<string, RoomInfo>();                            //�������X�g������
            
            roomListEntries = new Dictionary<string, GameObject>();                         //�����f�[�^������
        }

        // Update is called once per frame
        void Update()
        {
            ConnectInfoText.text = PhotonNetwork.NetworkClientState.ToString();             //�ʐM��Ԃ��
        }
        #region 0.�v���C���[���O���́A���O�C��
        //���O�C���{�^����������
        public void LoginBtn() 
        {
            if(PlayerNameInput.text=="")
            {
                NameErrorPage.SetActive(true);                                              //�G���[��
            }
            else
            {
                PhotonNetwork.LocalPlayer.NickName = PlayerNameInput.text;                  //���͂��ꂽ���O���ꎞ�I�Ƀj�b�N�l�[���ɕۑ�����

                PhotonNetwork.ConnectUsingSettings();                                       //Photon���ݒ���s��

                LoginPanel.SetActive(false);                                                //���O�C����ʂ����
            }
        }
        //�ʐM��Ԃ��X�V
        public override void OnConnectedToMaster()
        {
            OptionPage.SetActive(true);                                                     //���O�C�������㒼�ڕ����I�ԃy�[�W���J��
        }

        #endregion
        #region 2.���������
        public void CreateRoom() 
        {
            CreateRoomPanel.SetActive(true);                                                //�J��3.���������UI

            OptionPage.SetActive(false);                                                    //����2.���������/�I��UI

            RoomNameInput.text = "";                                                        //�����̖��O���͘g������
        }
        //�v���C���[��3.���������UI�ŕ����̖��O�����
        public void CreateRoomConBtn() 
        {
            string roomName = RoomNameInput.text;

            roomName = (roomName.Equals(string.Empty)) ? "Room" + Random.Range(1000, 10000) : roomName; //�v���C���[�������̖��O���͂��ĂȂ��ꍇ�����_���Ŗ��O�𐶐�

            RoomOptions options = new RoomOptions { MaxPlayers = 2, PlayerTtl = 10000 };                //�v���C���[�̍ő�l���A�v���C���[������������10000�b�ȓ��Đڑ��o����

            PhotonNetwork.CreateRoom(roomName, options, null);                                          //���������
        }
        //���������Ȃ�������2.���������/�I��UI�ɖ߂�
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            CreateRoomPanel.SetActive(false);                                                           //����3.���������UI

            OptionPage.SetActive(true);                                                                 //�J��2.���������/�I��UI
        }
        //2.���������/�I��UI����0.���O�C���y�[�W�ɖ߂�Ƃ�
        public void BackLoginPanel() 
        {
            PhotonNetwork.Disconnect();                                                                 //���ڑ���ؒf

            Application.LoadLevel(Application.loadedLevel);                                             //�����[�h����
        }
        #endregion
        #region 4.�������X�g���m�F����
        //2.�ŕ�����T����������4.�������X�gUI���J��
        public void RoomListBtn() 
        {
            if (!PhotonNetwork.InLobby)                                                     //Lobby�ɂ��Ȃ���
            {
                PhotonNetwork.JoinLobby();                                                  //Lobby�ɓ���
            }
            RoomListPage.SetActive(true);                                                   //�J��4.�������X�g

            OptionPage.SetActive(false);                                                    //����2.���������/�I��UI
        }
        //�������X�g��������/�X�V
        public override void OnJoinedLobby()
        {
           cachedRoomList.Clear();                                                     //�������X�g���폜����

           ClearRoomListView();                                                        //�������X�g��Prefab���폜����
        }
        //�������X�g�f�[�^���폜����
            void ClearRoomListView() 
        {
            foreach (GameObject entry in roomListEntries.Values)                        //roomListEntries�̃f�[�^���擾����
            {
                Destroy(entry.gameObject);                                              //�������X�g��Prefab���폜����
            }

            roomListEntries.Clear();                                                    //roomListEntries�̃f�[�^���X�V
        }
        //�������X�g�A�N�Z�X�ł��Ȃ���2.���������/�I��UI�ɖ߂�
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            OptionPage.SetActive(true);
            RoomListPage.SetActive(false);
        }
        //�������X�g�f�[�^���X�V����
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            ClearRoomListView();                                                        //�������X�g��Prefab���폜����

            UpdateCachedRoomList(roomList);                                             //�����f�[�^���X�V����

            UpdateRoomListView();                                                       //RoomListContent��Prefab���X�V����
        }
        void UpdateCachedRoomList(List<RoomInfo> roomList)
        {
            foreach (RoomInfo info in roomList)                                         //�������̕����̏�Ԃ͕����A�����Ȃ��A�����ꂽ���X�g���畔��Prefab���폜
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
                GameObject entry = Instantiate(RoomListPrefab);                                                     //����Prefab�𐶐�

                entry.transform.SetParent(RoomListContent.transform);                                               //�������ꂽ������RoomListContent�̎q���ɂ���

                entry.transform.localScale = Vector3.one;                                                           //�������ꂽ�����̃T�C�Y��������

                entry.GetComponent<RoomListEntry>().Initialize(info.Name, (byte)info.PlayerCount, (byte)info.MaxPlayers); //�����f�[�^��������

                roomListEntries.Add(info.Name, entry);                                                              //�����f�[�^��S�Ă̕����ɑ��
            }
        }

        #endregion
        #region 4.�������畔�������/�I�ԉ��
        public void BackBtn()
        {
            if (PhotonNetwork.InLobby)                                                      //����Lobby�ɂ���
            {
                PhotonNetwork.LeaveLobby();                                                 //Lobby����o��
            }
            RoomListPage.SetActive(false);                                                  //����4.�������X�gUI

            OptionPage.SetActive(true);                                                     //�J��2.���������/�I��UI
        }

        
        #endregion
    }
}
