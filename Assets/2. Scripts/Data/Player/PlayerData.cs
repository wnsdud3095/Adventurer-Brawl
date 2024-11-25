using UnityEngine;

namespace Jongmin
{
    [System.Serializable]
    public class PlayerData
    {
        public Character m_character_type;
        public PlayerStatus m_player_status;
        public int m_stage_id;
        public int m_stage_state;
        public int m_max_clear_stage;
        public int m_require_mob;
        public int m_talk_idx;
        

        public PlayerData(Character char_type, CharacterStatus character_status)
        {
            m_character_type = char_type;
            m_player_status = new PlayerStatus(character_status);
            m_stage_id = 0;
            m_stage_state = 0;
            m_require_mob = 0;
            m_talk_idx = 0;
            m_max_clear_stage = 0;
        }
    }
}