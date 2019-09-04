using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleScript : MonoBehaviour
{
    public struct NeighborBubbles
    {
        public BubbleScript NE;
        public BubbleScript NW;
        public BubbleScript E;
        public BubbleScript W;
        public BubbleScript SE;
        public BubbleScript SW;
    }

    public NeighborBubbles Neighbors;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }



}
