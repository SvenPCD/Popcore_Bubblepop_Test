using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BubbleScript : MonoBehaviour
{

    [HideInInspector]
    public int StepHeight = 0;

    public enum BubbleValue
    {
        Two,
        Four,
        Eight,
        Sixteen,
        ThirtyTwo,
        SixtyFour,
        OneHundred,
        TwoHundred,
        FiveHundred,
        OneK,
        TwoK,
        Count
    }

    public Text NumText;

    [HideInInspector]
    public BubbleValue ThisValue = BubbleValue.Two;

    public enum NeighborDirection
    { NE, NW, E, W, SE, SW, NONE }

    [HideInInspector]
    public BubbleScript NE;
    [HideInInspector]
    public BubbleScript NW;
    [HideInInspector]
    public BubbleScript E;
    [HideInInspector]
    public BubbleScript W;
    [HideInInspector]
    public BubbleScript SE;
    [HideInInspector]
    public BubbleScript SW;

    public BubbleValue SetValue(BubbleValue value)
    {
        ThisValue = value;
        SetBubbleLooks();

        return ThisValue;
    }

    public BubbleValue SetRandomValue(BubbleValue min, BubbleValue max)
    {
        int a = ((int)min < (int)max) ? (int)min : (int)max;
        int b = ((int)min < (int)max) ? (int)max : (int)min;

        ThisValue = (BubbleValue)Random.Range(a, b + 1);

        SetBubbleLooks();

        return ThisValue;
    }

    void SetBubbleLooks()
    {
        NumText.text = System.Math.Pow(2, (int)(ThisValue) + 1).ToString();
        this.GetComponent<SpriteRenderer>().color = BubbleManager.instance.BubbleColors[(int)ThisValue];

        if (this.GetComponent<TrailRenderer>() != null)
        {
            this.GetComponent<TrailRenderer>().startColor = BubbleManager.instance.BubbleColors[(int)ThisValue];
            Color endcolor = BubbleManager.instance.BubbleColors[(int)ThisValue];
            endcolor.a = 0;
            this.GetComponent<TrailRenderer>().endColor = endcolor;
        }
    }

    public void GrabNeighborSpheres()
    {
        Vector3 ThisPos = this.transform.position;
        float x = BubbleManager.xOffset;
        float y = BubbleManager.yOffset;

        var d = Physics2D.OverlapCircle(new Vector2(ThisPos.x + (x / 2), ThisPos.y + y), this.GetComponent<CircleCollider2D>().radius / 2, 1 << 8);
        NE = d?.GetComponent<BubbleScript>();

        d = Physics2D.OverlapCircle(new Vector2(ThisPos.x + x, ThisPos.y), this.GetComponent<CircleCollider2D>().radius / 2, 1 << 8);
        E = d?.GetComponent<BubbleScript>();

        d = Physics2D.OverlapCircle(new Vector2(ThisPos.x + (x / 2), ThisPos.y - y), this.GetComponent<CircleCollider2D>().radius / 2, 1 << 8);
        SE = d?.GetComponent<BubbleScript>();


        d = Physics2D.OverlapCircle(new Vector2(ThisPos.x - (x / 2), ThisPos.y + y), this.GetComponent<CircleCollider2D>().radius / 2, 1 << 8);
        NW = d?.GetComponent<BubbleScript>();

        d = Physics2D.OverlapCircle(new Vector2(ThisPos.x - x, ThisPos.y), this.GetComponent<CircleCollider2D>().radius / 2, 1 << 8);
        W = d?.GetComponent<BubbleScript>();

        d = Physics2D.OverlapCircle(new Vector2(ThisPos.x - (x / 2), ThisPos.y - y), this.GetComponent<CircleCollider2D>().radius / 2, 1 << 8);
        SW = d?.GetComponent<BubbleScript>();

    }

    public NeighborDirection GetBubbleDirection(Vector2 pos)
    {
        GrabNeighborSpheres();

        Vector2 dir = (new Vector2(this.transform.position.x, this.transform.position.y) - pos).normalized;
        float angle = Vector2.Angle(new Vector2(0, -1), dir);

        bool IsNegative = false;

        if (dir.x < 0)
            IsNegative = true;

        if (0 < angle && angle <= 60)
        {
            if (IsNegative)
            {
                if (NE == null)
                    return NeighborDirection.NE;
                else if (E == null)
                    return NeighborDirection.E;
                else
                    return NeighborDirection.NONE;
            }
            else
            {
                if (NW == null)
                    return NeighborDirection.NW;
                else if (W == null)
                    return NeighborDirection.W;
                else
                    return NeighborDirection.NONE;

            }
        }
        else if (60 < angle && angle <= 120)
        {
            if (IsNegative)
            {
                if (E == null)
                    return NeighborDirection.E;
                else if (SE == null)
                    return NeighborDirection.SE;
                else
                    return NeighborDirection.NONE;
            }
            else
            {
                if (W == null)
                    return NeighborDirection.W;
                else if (SW == null)
                    return NeighborDirection.SW;
                else
                    return NeighborDirection.NONE;
            }
        }
        else
        {
            if (IsNegative)
            {
                if (SE == null)
                    return NeighborDirection.SE;
                else
                    return NeighborDirection.NONE;
            }
            else
            {
                if (SW == null)
                    return NeighborDirection.SW;
                else
                    return NeighborDirection.NONE;
            }
        }
    }

    public Vector3 GetBubblePosition(NeighborDirection direction)
    {
        Vector3 ThisPos = this.transform.position;
        float x = BubbleManager.xOffset;
        float y = BubbleManager.yOffset;
        switch (direction)
        {
            case NeighborDirection.NE:
                return new Vector3(ThisPos.x + (x / 2), ThisPos.y + y, ThisPos.z);
            case NeighborDirection.NW:
                return new Vector3(ThisPos.x - (x / 2), ThisPos.y + y, ThisPos.z);
            case NeighborDirection.E:
                return new Vector3(ThisPos.x + x, ThisPos.y, ThisPos.z);
            case NeighborDirection.W:
                return new Vector3(ThisPos.x - x, ThisPos.y, ThisPos.z);
            case NeighborDirection.SE:
                return new Vector3(ThisPos.x + (x / 2), ThisPos.y - y, ThisPos.z);
            case NeighborDirection.SW:
                return new Vector3(ThisPos.x - (x / 2), ThisPos.y - y, ThisPos.z);
            default:
                return new Vector3();
        }
    }

    public bool GetNeighBorsOfvalue(BubbleValue value, ref List<BubbleScript> BubbleList)
    {
        bool NeighborAdded = false;
        if (NE != null && !BubbleList.Contains(NE))
            if (NE.ThisValue == value)
            {
                BubbleList.Add(NE);
                NeighborAdded = true;
            }

        if (NW != null && !BubbleList.Contains(NW))
            if (NW.ThisValue == value)
            {
                BubbleList.Add(NW);
                NeighborAdded = true;
            }

        if (W != null && !BubbleList.Contains(W))
            if (W.ThisValue == value)
            {
                BubbleList.Add(W);
                NeighborAdded = true;
            }

        if (E != null && !BubbleList.Contains(E))
            if (E.ThisValue == value)
            {
                BubbleList.Add(E);
                NeighborAdded = true;
            }

        if (SW != null && !BubbleList.Contains(SW))
            if (SW.ThisValue == value)
            {
                BubbleList.Add(SW);
                NeighborAdded = true;
            }

        if (SE != null && !BubbleList.Contains(SE))
            if (SE.ThisValue == value)
            {
                BubbleList.Add(SE);
                NeighborAdded = true;
            }

        return NeighborAdded;
    }


    public void Pop()
    {
        BubbleManager.Points += (int)System.Math.Pow(2, (int)(ThisValue) + 1);
        Destroy(this.gameObject);
    }

    public void Explode()
    {
        if (NE != null) NE.Pop();
        if (SE != null) SE.Pop();
        if (NW != null) NW.Pop();
        if (SW != null) SW.Pop();
        if (E != null) E.Pop();
        if (W != null) W.Pop();

        Pop();
    }


    bool IsAlone()
    {
        bool Alone = true;

        if (NE != null) Alone = false;
        if (SE != null) Alone = false;
        if (NW != null) Alone = false;
        if (SW != null) Alone = false;
        if (E != null) Alone = false;
        if (W != null) Alone = false;
        return Alone;
    }

    public void GetAllNeighbors(ref List<BubbleScript> BubbleList)
    {
        if (NE != null && !BubbleList.Contains(NE)) BubbleList.Add(NE);
        if (NW != null && !BubbleList.Contains(NW)) BubbleList.Add(NW);
        if (W != null && !BubbleList.Contains(W))BubbleList.Add(W);
        if (E != null && !BubbleList.Contains(E))BubbleList.Add(E);
        if (SW != null && !BubbleList.Contains(SW))BubbleList.Add(SW);
        if (SE != null && !BubbleList.Contains(SE))BubbleList.Add(SE);
    }


}
