using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
    [Header("Tiles&Tilemap References")]
    [Header("Options")]
    [SerializeField] private bool observeMovementPenalties = true;

    //移动开销
    [Range(0, 20)]
    [SerializeField] private int pathMovementPenalty = 0;
    [Range(0, 20)]
    [SerializeField] private int defaultMovementPenalty = 0;

    private GridNodes gridNodes;
    private Node startNode;
    private Node targetNode;
    private int gridWidth;
    private int gridHeight;
    private int originX;
    private int originY;

    private List<Node> openNodeList;
    private HashSet<Node> closedNodeList;

    private bool pathFound = false;

    public bool BuildPath(SceneName sceneName, Vector2Int startGridPosition, Vector2Int targetGridPosition, Stack<NPCMovementStep> nPCMovementSteps)
    {
        if (PopulateGridNodesFromGridPropertiesDictionary(sceneName, startGridPosition, targetGridPosition))
        {
            if (FindShortestPath())
            {
                UpdatePathOnNPCMovementStepStack(sceneName, nPCMovementSteps);//设置npc路径
                return true;
            }
        }
        return false;
    }

    private void UpdatePathOnNPCMovementStepStack(SceneName sceneName, Stack<NPCMovementStep> nPCMovementSteps)
    {
        Node nextNode = targetNode;

        while (nextNode != null)
        {
            NPCMovementStep nPCMovementStep = new NPCMovementStep();

            nPCMovementStep.sceneName = sceneName;
            nPCMovementStep.gridCoordinate = new Vector2Int(nextNode.gridPosition.x + originX, nextNode.gridPosition.y + originY);

            nPCMovementSteps.Push(nPCMovementStep);
            nextNode = nextNode.parentNode;
        }
    }
    private bool PopulateGridNodesFromGridPropertiesDictionary(SceneName sceneName, Vector2Int startGridPosition, Vector2Int targetGridPosition)
    {
        SceneSave sceneSave;
        //获取网格属性字典
        if (GridPropertiesManager.Instance.GameObjectSave.sceneData.TryGetValue(sceneName.ToString(), out sceneSave))
        {
            if (sceneSave.gridPropertyDetailsDictionary != null)
            {
                //获取网格节点数据
                if (GridPropertiesManager.Instance.GetGridDimensions(sceneName, out Vector2Int gridDimensions, out Vector2Int gridOrigin))
                {
                    gridNodes = new GridNodes(gridDimensions.x, gridDimensions.y);
                    gridWidth = gridDimensions.x;
                    gridHeight = gridDimensions.y;
                    originX = gridOrigin.x;
                    originY = gridOrigin.y;

                    openNodeList = new List<Node>();
                    closedNodeList = new HashSet<Node>();
                }
                else
                {
                    return false;
                }
                startNode = gridNodes.GetGridNode(startGridPosition.x - gridOrigin.x, startGridPosition.y - gridOrigin.y);
                targetNode = gridNodes.GetGridNode(targetGridPosition.x - gridOrigin.x, targetGridPosition.y - gridOrigin.y);

                //更新每个节点的属性
                for (int x = 0; x < gridDimensions.x; ++x)
                {
                    for (int y = 0; y < gridDimensions.y; ++y)
                    {
                        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(x + gridOrigin.x, y + gridOrigin.y, sceneSave.gridPropertyDetailsDictionary);

                        if (gridPropertyDetails != null)
                        {
                            if (gridPropertyDetails.isNPCObstacle == true)
                            {
                                Node node = gridNodes.GetGridNode(x, y);
                                node.isObstacle = true;
                            }
                            else if (gridPropertyDetails.isPath == true)
                            {
                                Node node = gridNodes.GetGridNode(x, y);
                                node.movementPenalty = pathMovementPenalty;
                            }
                            else
                            {
                                Node node = gridNodes.GetGridNode(x, y);
                                node.movementPenalty = defaultMovementPenalty;
                            }
                        }
                    }
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
        return true;
    }

    private bool FindShortestPath()
    {
        openNodeList.Add(startNode);

        while (openNodeList.Count > 0)
        {
            openNodeList.Sort();
            //将fCost最小节点从openlist加入closedlist
            Node currentNode = openNodeList[0];
            openNodeList.RemoveAt(0);
            closedNodeList.Add(currentNode);

            if (currentNode == targetNode)
            {
                pathFound = true;
                break;
            }
            //依次检查周围节点
            EvaluateCurrentNodeNeighbours(currentNode);
        }

        if (pathFound)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void EvaluateCurrentNodeNeighbours(Node currentNode)
    {
        Vector2Int currentNodeGridPosition = currentNode.gridPosition;

        Node validNeighbourNode;
        //遍历周围8格
        for (int i = -1; i <= 1; ++i)
        {
            for (int j = -1; j <= 1; ++j)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }
                //邻居节点
                validNeighbourNode = GetValidNodeNeighbour(currentNodeGridPosition.x + i, currentNodeGridPosition.y + j);

                if (validNeighbourNode != null)
                {
                    //更新邻居gCost
                    int newCostToNeighbour;

                    if (observeMovementPenalties)
                    {
                        newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighbourNode) + validNeighbourNode.movementPenalty;
                    }
                    else
                    {
                        newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighbourNode);
                    }

                    bool isValidNeighbourNodeInOpenList = openNodeList.Contains(validNeighbourNode);

                    if (newCostToNeighbour < validNeighbourNode.gCost || !isValidNeighbourNodeInOpenList)
                    {
                        validNeighbourNode.gCost = newCostToNeighbour;
                        validNeighbourNode.hCost = GetDistance(validNeighbourNode, targetNode);

                        validNeighbourNode.parentNode = currentNode;

                        if (!isValidNeighbourNodeInOpenList)
                        {
                            openNodeList.Add(validNeighbourNode);
                        }
                    }
                }
            }
        }
    }

    private Node GetValidNodeNeighbour(int x, int y)
    {
        //超出范围、障碍物、已在closedlist中节点全部无效
        if (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight)
        {
            return null;
        }

        Node node = gridNodes.GetGridNode(x, y);

        if (node.isObstacle || closedNodeList.Contains(node))
        {
            return null;
        }
        else
        {
            return node;
        }
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int xDistance = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int yDistance = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        if (xDistance > yDistance)//存在对角
        {
            return 14 * yDistance + 10 * (xDistance - yDistance);
        }
        else
        {
            return 14 * xDistance + 10 * (yDistance - xDistance);
        }
    }
}
