using Jongmin;
using System.Collections.Generic;
using System.IO;
using Taekyung;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Junyoung
{

    public class StageManager : MonoBehaviour
    {


        private List<StageData> m_stages_data;

        [Header("Stage UI")]
        [SerializeField]
        private Button[] m_select_buttons; //인스펙터에서 연결

        [SerializeField]
        private RectTransform[] m_select_buttons_pos_list;

        [SerializeField]
        private GameObject m_stage_select_UI;//인스펙터에서 연결

        [SerializeField]
        private GameObject m_stage_select_ckeck_UI;//인스펙터에서 연결

        [SerializeField]
        private TMP_Text m_ui_text;//인스펙터에서 연결

        [SerializeField]
        private RectTransform m_player_icon;//인스펙터에서 연결

        private int m_stage_index;
        public int m_max_stage { get; private set; } = 9;

        private GameObject m_player;


        [SerializeField]
        private int m_now_button_index = 0;
        private List<int> m_path_list = new List<int>();
        private int m_current_path_index = 0;
        private bool m_is_icon_moving = false;
        private float m_icon_move_speed = 250f;





        [Header("Managers")]
        [SerializeField]
        private TalkManager m_talk_manager;

        [SerializeField]
        private SaveManager m_save_manager;

        private CameraMoveCtrl m_camera_move_ctrl;

        void Start()
        {
            m_camera_move_ctrl = Camera.main.GetComponent<CameraMoveCtrl>();

            m_player = GameObject.FindGameObjectWithTag("Player");
            m_save_manager = GameObject.FindAnyObjectByType<SaveManager>();
            LoadStagesData("StageData.json");

        }


        private IEnumerator MoveIconCorutine()
        {

            while (m_is_icon_moving && m_path_list.Count > 0)
            {
                if (m_current_path_index < 0 || m_current_path_index >= m_path_list.Count)
                {
                    Debug.LogError($"m_current_path_index 범위 초과: {m_current_path_index}, m_path_list.Count={m_path_list.Count}");
                    m_is_icon_moving = false;
                    yield break;
                }

                int target_index = m_path_list[m_current_path_index]; // 만들어진 경로 리스트를 path_index값을 따라서 하나씩 이동

                if (target_index < 0 || target_index >= m_select_buttons_pos_list.Length)
                {
                    Debug.LogError($"target_index 범위 초과: {target_index}, m_select_buttons_pos_list.Length={m_select_buttons_pos_list.Length}");
                    m_is_icon_moving = false;
                    yield break;
                }
                Debug.Log($"MoveIconCorutine 정상 실행");

                Vector2 target_pos = m_select_buttons_pos_list[target_index].anchoredPosition;

                target_pos += new Vector2(0, 70); // 아이콘이 버튼을 가리지 않도록 70만큼 offset


                //아이콘이 클릭한 버튼 위치로 이동 
                // 부동 소수점 오류 때문에 ==로 단순 비교는 오류가 발생 할 수 있음
                while (Vector2.Distance(m_player_icon.anchoredPosition, target_pos) > 0.1f)
                {
                    m_player_icon.anchoredPosition = Vector2.MoveTowards(m_player_icon.anchoredPosition, target_pos, m_icon_move_speed * Time.deltaTime);
                    yield return null; //다음 프레임까지 대기, 프레임 단위로 부드럽게 이동시키기 위해 사용
                }
                //다음 경로로 이동
                if (m_current_path_index < m_path_list.Count - 1) // 버튼에 도착한 이후에도 index++하면 index범위 오류가 발생함
                    m_current_path_index++;
                else //최종 도착
                {
                    m_is_icon_moving = false;

                    m_stage_select_ckeck_UI.SetActive(true);

                    Debug.Log($"아이콘이 버튼 {m_now_button_index}에 도달");
                }
            }
        }

        private void LoadStagesData(string file_name)
        {
            string file_path = Path.Combine(Application.streamingAssetsPath, file_name);

            if (!File.Exists(file_path))
            {
                Debug.LogError($"JSON 파일이 없음 {file_path}");
                return;
            }

            string json_data = File.ReadAllText(file_path);

            StageDataWrapper wrapper = JsonUtility.FromJson<StageDataWrapper>(json_data);

            if (wrapper == null || wrapper.StageData == null)
            {
                Debug.LogError("JSON 파싱 실패, 데이터가 유효하지 않음");
                return;
            }
            m_stages_data = new List<StageData>(wrapper.StageData);


            Debug.Log(json_data);

            Debug.Log("스테이지 데이터 로드 성공");
        }

        public void LoadStage(int stage_index) //버튼에서 로드할 경우 인덱스는 인스펙터에서 버튼마다 직접 할당
        {
            if (stage_index < 0 || stage_index >= m_stages_data.Count)
            {
                Debug.LogError("잘못된 스테이지 인덱스");
                return;
            }

            StageData stageData = m_stages_data[stage_index];

            // 플레이어 위치 설정
            m_player.transform.position = new Vector3(
                                                        stageData.m_player_start_position.x,
                                                        stageData.m_player_start_position.y,
                                                        m_player.transform.position.z
                                                     );

            // 카메라 제한 설정
            m_camera_move_ctrl.CameraLimitCenter = stageData.m_camera_limit_center;
            m_camera_move_ctrl.CameraLimitSize = stageData.m_camera_limit_size;

            Debug.Log($"스테이지 {stage_index} 로드");


            m_save_manager.Player.m_stage_id = stage_index;

            m_save_manager.Player.m_stage_state = 0;
            m_talk_manager.ChangeTalkScene();


            Debug.Log($"m_stage_id : {m_save_manager.Player.m_stage_id}");


        }

        public void StageSelectPanelOnoff() // 스테이지 선택 UI를 활성화/비활성화 함
        {
            bool isActive = m_stage_select_UI.activeSelf;
            if (!isActive)
            {
                SelectButtonInteract();
                Debug.Log($"스테이지 선택창 활성화");
            }

            else
                Debug.Log($"스테이지 선택창 비활성화");
            m_stage_select_UI.SetActive(!isActive);

            if (m_stage_select_ckeck_UI.activeSelf)
                m_stage_select_ckeck_UI.SetActive(false);


        }

        public void StageSelect(int stage_index)// 버튼 클릭으로 버튼에 해당하는 스테이지 index를 받아옴
        {
            m_stage_index = stage_index;
            m_ui_text.text = $"Do you want to go to Stage {m_stage_index + 1} ?";

        }

        public void StageSelectYes() //m_stage_index에 맞게 스테이지를 불러옴
        {
            Debug.Log($"스테이지 선택 예 클릭");
            LoadStage(m_stage_index);
            m_stage_select_ckeck_UI.SetActive(false);
            StageSelectPanelOnoff();
        }

        public void StageSelectNo() // 다시 스테이지 선택 UI로 돌아감
        {
            Debug.Log($"스테이지 선택 아니오 클릭");
            m_stage_select_ckeck_UI.SetActive(false);
        }

        public void SelectButtonInteract() //스테이지 선택 버튼을 최대 클리어 스테이지 +1 만큼 활성화 
        {
            for (int i = 1; i <= m_save_manager.Player.m_max_clear_stage + 1; i++)
            {
                if (i > m_max_stage)
                {
                    Debug.Log($"더 활성화할 버튼이 없음");
                    return;
                }
                m_select_buttons[i].interactable = true;
            }
            Debug.Log($"스테이지 {m_save_manager.Player.m_max_clear_stage + 1} 까지 버튼 활성화");
        }

        public void SelectButtonReset() //활성화된 버튼들을 전부 비활성화
        {
            for (int i = 1; i <= 9; i++)
            {
                m_select_buttons[i].interactable = false;
            }

            Debug.Log($"스테이지 선택 버튼 비활성화");
        }


        public void ClickedButtonIndex(int button_index) //클릭된 버튼의 index값을 불러와서 지금 위치와 가려는 위치를 비교하여 정방향/역방향 경로를 리스트에 추가
        {
            Debug.Log($"ClickedButtonPosiotion 호출됨: button_index={button_index}, m_now_button_index={m_now_button_index}");
            if (button_index < 0 || button_index >= m_select_buttons.Length)
            {
                Debug.LogError($"잘못된 button_index: {button_index}, 유효 범위: 0 ~ {m_select_buttons.Length - 1}");
                return;
            }

            m_path_list.Clear();
            m_current_path_index = 0;

            if (m_now_button_index < button_index) // 지금 위치보다 클릭한 버튼이 뒤면 정방향
            {
                for (int i = m_now_button_index + 1; i <= button_index; i++)
                {
                    Debug.Log($"m_path_list 추가: {i}");
                    m_path_list.Add(i);
                }
            }
            else if (m_now_button_index > button_index) // 역방향
            {
                for (int i = m_now_button_index - 1; i >= button_index; i--)
                {
                    Debug.Log($"m_path_list 추가: {i}");
                    m_path_list.Add(i);
                }
            }
            else
            {
                Debug.Log("같은 버튼 클릭: m_now_button_index 추가");
                m_path_list.Add(m_now_button_index); // 같은 버튼을 클릭한 경우 현재 위치 index를 넘겨줘서 그자리에 있도록
            }
            m_is_icon_moving = true;

            m_now_button_index = button_index; // 경로는 만들어졌으니 now_button_index 를 목표 버튼의 index로 초기화
            Debug.Log($"경로 생성 완료: {string.Join(", ", m_path_list)}");

            StartCoroutine(MoveIconCorutine());
        }
    }
}