using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonAssault.Dungeons
{
    /// <summary>
    /// 
    /// </summary>
    public class Dungeon : MonoBehaviour
    {
        public Vector3 blockX_MeshSize;

        [Range(0, 255)]
        [SerializeField]
        public byte BlocksCount;
        public DungeonBlock[,] blocks;
        [Range(.1f, 1f)]
        [SerializeField]
        public float scaleFactor;

        public GameObject[] blocksS;
        public GameObject[] blocksL;
        public GameObject[] blocksI;
        public GameObject[] blocksT;
        public GameObject[] blocksX;

        public void Start()
        {
            ShowDungeon(CorrectDungeon(GenerateDungeon()));
        }

        /// <summary>
        /// Заполняем комнаты черепашкой.
        /// </summary>
        public List<DungeonBlock> GenerateDungeon()
        {
            Vector2Int dungeonSize = new Vector2Int(BlocksCount*2 + 1, BlocksCount * 2 + 1);
            List<Vector2Int> cursors = new List<Vector2Int>();
            Vector2Int step = new Vector2Int(0, 0);
            cursors.Add(new Vector2Int((int)(dungeonSize.x*0.5f), (int)(dungeonSize.y*0.5f)));
            blocks = new DungeonBlock[dungeonSize.x, dungeonSize.y];
            List<DungeonBlock> blockList = new List<DungeonBlock>();
            for (int n = 0; n < BlocksCount; n++)
            {
                switch (Random.Range(0, 3))
                {
                    case 0: step = new Vector2Int(-1, 0); break;
                    case 1: step = new Vector2Int(0, 1); break;
                    case 2: step = new Vector2Int(1, 0); break;
                    case 3: step = new Vector2Int(0, -1); break;
                }
                if (cursors[0].x < 1 || cursors[0].x > dungeonSize.x - 2)
                {
                    step = new Vector2Int(-step.x, 0);
                }
                if (cursors[0].y < 1 || cursors[0].y > dungeonSize.y - 2)
                {
                    step = new Vector2Int(0, -step.y);
                }
                cursors[0] += step;
                blocks[cursors[0].x, cursors[0].y] = new DungeonBlock()
                {
                    t = DungeonBlockType.I,
                    x = cursors[0].x, y = cursors[0].y,
                };
                blockList.Add(blocks[cursors[0].x, cursors[0].y]);
            }
            return blockList;
        }

        /// <summary>
        /// Определяем ориентацию блоков в подземелье.
        /// </summary>
        /// <param name="blockList"></param>
        /// <returns></returns>
        public List<DungeonBlock> CorrectDungeon(List<DungeonBlock> blockList)
        {
            for (int n = 0; n < blockList.Count; n++)
            {
                DungeonBlockNeighbors nei = CheckNeighbors(blockList[n]);
                string s = System.Convert.ToString((byte)(nei), 2).PadLeft(5, '0');
                switch ((byte)nei)
                {
                    /*0001*/case 1: blockList[n].t = DungeonBlockType.S; blockList[n].r = DungeonBlockRotation.r_180; break;
                    /*0010*/case 2: blockList[n].t = DungeonBlockType.S; blockList[n].r = DungeonBlockRotation.r_270; break;
                    /*0100*/case 4: blockList[n].t = DungeonBlockType.S; blockList[n].r = DungeonBlockRotation.r_0; break;
                    /*1000*/case 8: blockList[n].t = DungeonBlockType.S; blockList[n].r = DungeonBlockRotation.r_90; break;

                    /*0101*/case 5: blockList[n].t = DungeonBlockType.I; blockList[n].r = DungeonBlockRotation.r_0; break;
                    /*1010*/case 10: blockList[n].t = DungeonBlockType.I; blockList[n].r = DungeonBlockRotation.r_90; break;

                    /*0011*/case 3: blockList[n].t = DungeonBlockType.L; blockList[n].r = DungeonBlockRotation.r_180; break;
                    /*0110*/case 6: blockList[n].t = DungeonBlockType.L; blockList[n].r = DungeonBlockRotation.r_270; break;
                    /*1100*/case 12: blockList[n].t = DungeonBlockType.L; blockList[n].r = DungeonBlockRotation.r_0; break;
                    /*1001*/case 9: blockList[n].t = DungeonBlockType.L; blockList[n].r = DungeonBlockRotation.r_90; break;

                    /*0111*/case 7: blockList[n].t = DungeonBlockType.T; blockList[n].r = DungeonBlockRotation.r_180; break;
                    /*1011*/case 11: blockList[n].t = DungeonBlockType.T; blockList[n].r = DungeonBlockRotation.r_90; break;
                    /*1101*/case 13: blockList[n].t = DungeonBlockType.T; blockList[n].r = DungeonBlockRotation.r_0; break;
                    /*1110*/case 14: blockList[n].t = DungeonBlockType.T; blockList[n].r = DungeonBlockRotation.r_270; break;

                    /*1111*/case 15: blockList[n].t = DungeonBlockType.X; break;
                }
                //Debug.Log(s + "  " + (DungeonBlockType)(blockList[n].t));
            }
            return blockList;
        }

        /// <summary>
        /// Определяем тип комнаты с помощью соседних комнат.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public DungeonBlockNeighbors CheckNeighbors(DungeonBlock block)
        {
            DungeonBlockNeighbors n = (DungeonBlockNeighbors)0;
            n |= (blocks[block.x-1, block.y] != null) ? DungeonBlockNeighbors.Back : 0;
            n |= (blocks[block.x, block.y-1] != null) ? DungeonBlockNeighbors.Left : 0;
            n |= (blocks[block.x+1, block.y] != null) ? DungeonBlockNeighbors.Front : 0;
            n |= (blocks[block.x, block.y+1] != null) ? DungeonBlockNeighbors.Right : 0;
            block.code = n;
            return n;
        }


        /// <summary>
        /// Отрисовываем подземелье префабами блоков.
        /// </summary>
        /// <param name="blockList"></param>
        public void ShowDungeon(List<DungeonBlock> blockList)
        {
            int num = 0;


            Vector3 entranceShift = new Vector3(blockList[0].x * blockX_MeshSize.x, 0, blockList[0].y * blockX_MeshSize.z);
            for (int n = 0; n < blockList.Count; n++)
            {
                var b = blockList[n];
                Transform newBlock = null;
                switch (b.t)
                {
                    case DungeonBlockType.S: newBlock = Instantiate(blocksS[Random.Range(0, blocksS.Length)], transform).transform; break;
                    case DungeonBlockType.L: newBlock = Instantiate(blocksL[Random.Range(0, blocksL.Length)], transform).transform; break;
                    case DungeonBlockType.I: newBlock = Instantiate(blocksI[Random.Range(0, blocksI.Length)], transform).transform; break;
                    case DungeonBlockType.T: newBlock = Instantiate(blocksT[Random.Range(0, blocksT.Length)], transform).transform; break;
                    case DungeonBlockType.X: newBlock = Instantiate(blocksX[Random.Range(0, blocksX.Length)], transform).transform; break;
                }
                newBlock.name = newBlock.name.Replace("(Clone)", (num++).ToString() + "   " + b.code + "  " +
                    System.Convert.ToString((byte)(b.code), 2).PadLeft(4, '0') + "   " + b.t + "   " +b.r);
                newBlock.localPosition = new Vector3(b.x * blockX_MeshSize.x, 0, b.y * blockX_MeshSize.z)
                    - entranceShift;
                newBlock.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                switch (b.r)
                {
                    case DungeonBlockRotation.r_0: newBlock.localEulerAngles = new Vector3(0, 0, 0); break;
                    case DungeonBlockRotation.r_90: newBlock.localEulerAngles = new Vector3(0, 90, 0); break;
                    case DungeonBlockRotation.r_180: newBlock.localEulerAngles = new Vector3(0, 180, 0); break;
                    case DungeonBlockRotation.r_270: newBlock.localEulerAngles = new Vector3(0, 270, 0); break;
                }
            }
        }
    }
}