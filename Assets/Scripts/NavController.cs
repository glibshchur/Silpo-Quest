using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NavController : MonoBehaviour {

    public AStar AStar;
    private Transform destination;
    private bool _initialized = false;
    private bool _initializedComplete = false;
    private List<Node> path = new List<Node>();
    private int currNodeIndex = 0;
    private float maxDistance = 1.1f;

    private void Start() {
#if UNITY_EDITOR
        InitializeNavigation();
#endif
    }
    
    Node ReturnClosestNode(Node[] nodes, Vector3 point) {
        float minDist = Mathf.Infinity;
        Node closestNode = null;
        foreach (Node node in nodes) {
            float dist = Vector3.Distance(node.pos, point);
            if (dist < minDist) {
                closestNode = node;
                minDist = dist;
            }
        }
        return closestNode;
    }

    public void InitializeNavigation() {
        StopAllCoroutines();
        StartCoroutine(DelayNavigation());
    }

    IEnumerator DelayNavigation() {
        while(FindObjectOfType<DiamondBehavior>() == null){
            yield return new WaitForSeconds(.5f);
            Debug.Log("waiting for shapes to load...");
        }
        InitNav();
    }

    void InitNav(){
        if (!_initialized) {
            _initialized = true;
            Debug.Log("Initializing Navigation");
            Node[] allNodes = FindObjectsOfType<Node>();
            Debug.Log("NODES: " + allNodes.Length);
            Node closestNode = ReturnClosestNode(allNodes, transform.position);
            Debug.Log("closest: " + closestNode.gameObject.name);
            Node target = FindObjectOfType<DiamondBehavior>().GetComponent<Node>();
            Debug.Log("target: " + target.gameObject.name);
            foreach (Node node in allNodes) {
                node.FindNeighbors(maxDistance);
            }
            
            path = AStar.FindPath(closestNode, target, allNodes);

            if (path == null) {
                maxDistance += .1f;
                Debug.Log("Increasing search distance: " + maxDistance);
                _initialized = false;
                InitNav();
                return;
            }
            
            for (int i = 0; i < path.Count - 1; i++) {
                path[i].NextInList = path[i + 1];
            }
            
            path[0].Activate(true);
            _initializedComplete = true;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (_initializedComplete && other.CompareTag("waypoint")) {
            currNodeIndex = path.IndexOf(other.GetComponent<Node>());
            if (currNodeIndex < path.Count - 1) {
                path[currNodeIndex + 1].Activate(true);
            }
        }
    }
}
