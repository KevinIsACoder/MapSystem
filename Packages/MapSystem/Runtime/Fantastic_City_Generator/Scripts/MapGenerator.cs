using UnityEngine;
using Random = UnityEngine.Random;

namespace FCG
{
    public class MapGenerator : MonoBehaviour
    {
        private CityGenerator m_cityGenerator;
        public GameObject m_generateObj;
        public bool generateOnStart = false;

        public float mapWidth; //地图宽度
        public float mapHeight; //地图高度
        public delegate void GenerateCompleteDelegate();
        public event GenerateCompleteDelegate HandleGenerateComplete;
        
        private void Awake()
        {
            m_cityGenerator = m_generateObj.GetComponent<CityGenerator>();
            m_cityGenerator.OnGenerateCityComplete = () =>
            {
                HandleGenerateComplete?.Invoke();
            };
        }

        private void Start()
        {
            if (generateOnStart)
            {
                GenerateMap();
            }
        }

        public void GenerateMap()
        {
            var size = Random.Range(1, 5);
            m_cityGenerator.GenerateCity(size);
            float downTownSize = 100;
            m_cityGenerator.GenerateAllBuildings(false, downTownSize);
        }
    }   
}
