using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BubbleManager : MonoBehaviour
{
    public static float xOffset = 150f;
    public static float yOffset = 130f;
    public static int Points = 0;

    public Transform TopLeftCornerPivot;

    private bool OddRow = false;
    private int CurrentRows = 0;

    public static BubbleManager instance;

    float BubbleSpeed = 1000f;

    private enum ShootState
    {
        Aiming,
        ShotMoving,
        MergingBubbles,
        RemovingfloatingBubbles,
        AddingMoreBubbles,
        Reload
    }

    ShootState currentState = ShootState.Aiming;

    [SerializeField]
    GameObject NextBubble;
    [SerializeField]
    GameObject CurrentBubble;

    [SerializeField]
    GameObject BubblePrefab;

    [SerializeField]
    GameObject BubbleHolder;

    [SerializeField]
    Text ScoreText;

    BubbleScript.BubbleValue NextBubbleValue;
    BubbleScript.BubbleValue CurrentBubbleValue;

    [SerializeField]
    LineRenderer TargetingLine;

    [HideInInspector]
    public List<Vector3> TargetLine = new List<Vector3>();


    public Color[] BubbleColors = new Color[11];

    bool BubbleHit = false;

    [SerializeField]
    GameObject TargetBalloon;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;

        CurrentBubbleValue = CurrentBubble.GetComponent<BubbleScript>().SetRandomValue(BubbleScript.BubbleValue.Two, BubbleScript.BubbleValue.SixtyFour);
        NextBubbleValue = NextBubble.GetComponent<BubbleScript>().SetRandomValue(BubbleScript.BubbleValue.Two, BubbleScript.BubbleValue.SixtyFour);


        SpawnBubbles();
    }

    void Reload()
    {
        SetNeighborBubbles();
        CurrentBubbleValue = CurrentBubble.GetComponent<BubbleScript>().SetValue(NextBubbleValue);
        NextBubbleValue = NextBubble.GetComponent<BubbleScript>().SetRandomValue(BubbleScript.BubbleValue.Two, BubbleScript.BubbleValue.SixtyFour);
        currentState = ShootState.Aiming;
    }

    // Update is called once per frame
    void Update()
    {

        ScoreText.text = Points.ToString();
        switch (currentState)
        {
            case ShootState.Aiming:

#if UNITY_EDITOR
                AimingEditor();
#endif
#if UNITY_ANDROID
                AimingPhone();
#endif
                break;
            case ShootState.ShotMoving:
                BubbleMoving();
                break;
            case ShootState.MergingBubbles:
                MergeBubbles();
                break;
            case ShootState.RemovingfloatingBubbles:
                DestroyFloating();
                break;
            case ShootState.AddingMoreBubbles:
                SpawnBubbles();
                break;
            case ShootState.Reload:
                Reload();
                break;
            default:
                break;
        }
    }


    void AimingEditor()
    {
        if (Input.GetMouseButton(0))
        {
            TargetLine = new List<Vector3>();

            Vector3 point = new Vector3();
            Event currentEvent = Event.current;
            Vector2 mousePos = Input.mousePosition;

            point = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));

            Vector2 origin = new Vector2(TargetingLine.transform.position.x, TargetingLine.transform.position.y);
            Vector2 direction = (new Vector2(point.x, point.y) - new Vector2(TargetingLine.transform.position.x, TargetingLine.transform.position.y)).normalized;

            TargetLine.Add(TargetingLine.transform.position + (new Vector3(direction.x, direction.y, 0) * 75));

            var Ray = Physics2D.Raycast(origin, direction, Mathf.Infinity);
            if (Ray.collider != null)
            {
                TargetLine.Add(new Vector3(Ray.point.x, Ray.point.y, TargetingLine.transform.position.z));

                if (Ray.transform != null && Ray.transform.tag == "Bubble")
                {
                    if (Ray.transform.GetComponent<BubbleScript>().GetBubbleDirection(Ray.point) != BubbleScript.NeighborDirection.NONE)
                    {
                        TargetingLine.positionCount = TargetLine.Count;
                        TargetingLine.SetPositions(TargetLine.ToArray());
                        TargetingLine.enabled = true;
                        TargetBalloon.SetActive(true);
                        TargetBalloon.transform.position = Ray.transform.GetComponent<BubbleScript>().GetBubblePosition(Ray.transform.GetComponent<BubbleScript>().GetBubbleDirection(Ray.point));
                        BubbleHit = true;
                    }
                    else
                    {
                        TargetingLine.enabled = false;
                        TargetBalloon.SetActive(false);
                        BubbleHit = false;
                    }
                }
                else
                {
                    origin = Ray.point;
                    direction.x = direction.x * -1;
                    origin += direction;
                    Ray = Physics2D.Raycast(origin, direction, Mathf.Infinity);
                    if (Ray.transform != null && Ray.transform.tag == "Bubble")
                    {
                        if (Ray.transform.GetComponent<BubbleScript>().GetBubbleDirection(Ray.point) != BubbleScript.NeighborDirection.NONE)
                        {
                            TargetLine.Add(new Vector3(Ray.point.x, Ray.point.y, TargetingLine.transform.position.z));
                            TargetingLine.positionCount = TargetLine.Count;
                            TargetingLine.SetPositions(TargetLine.ToArray());
                            TargetingLine.enabled = true;
                            TargetBalloon.SetActive(true);
                            TargetBalloon.transform.position = Ray.transform.GetComponent<BubbleScript>().GetBubblePosition(Ray.transform.GetComponent<BubbleScript>().GetBubbleDirection(Ray.point));
                            BubbleHit = true;
                        }
                        else
                        {
                            TargetingLine.enabled = false;
                            TargetBalloon.SetActive(false);
                            BubbleHit = false;
                        }
                    }
                    else
                    {
                        TargetingLine.enabled = false;
                        TargetBalloon.SetActive(false);
                        BubbleHit = false;
                    }
                }
            }
        }
        else if (Input.GetMouseButtonUp(0) && BubbleHit)
        {
            TargetLine[0] = CurrentBubble.transform.position;
            TargetLine[TargetLine.Count - 1] = TargetBalloon.transform.position;
            TargetBalloon.SetActive(false);
            TargetingLine.enabled = false;
            currentIndex = 0;

            if (CurrentBubble.GetComponent<TrailRenderer>() != null)
                CurrentBubble.GetComponent<TrailRenderer>().enabled = true;

            currentState = ShootState.ShotMoving;
        }
    }

    void AimingPhone()
    {
        if (Input.touchCount > 0)
        {
            var d = Input.touches[0];
            if (d.phase == TouchPhase.Began || d.phase == TouchPhase.Moved || d.phase == TouchPhase.Stationary)
            {
                TargetLine = new List<Vector3>();

                Vector3 point = new Vector3();
                Event currentEvent = Event.current;
                Vector2 mousePos = d.position;

                point = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));

                Vector2 origin = new Vector2(TargetingLine.transform.position.x, TargetingLine.transform.position.y);
                Vector2 direction = (new Vector2(point.x, point.y) - new Vector2(TargetingLine.transform.position.x, TargetingLine.transform.position.y)).normalized;

                TargetLine.Add(TargetingLine.transform.position + (new Vector3(direction.x, direction.y, 0) * 75));

                var Ray = Physics2D.Raycast(origin, direction, Mathf.Infinity);
                if (Ray.collider != null)
                {
                    TargetLine.Add(new Vector3(Ray.point.x, Ray.point.y, TargetingLine.transform.position.z));

                    if (Ray.transform != null && Ray.transform.tag == "Bubble")
                    {
                        if (Ray.transform.GetComponent<BubbleScript>().GetBubbleDirection(Ray.point) != BubbleScript.NeighborDirection.NONE)
                        {
                            TargetingLine.positionCount = TargetLine.Count;
                            TargetingLine.SetPositions(TargetLine.ToArray());
                            TargetingLine.enabled = true;
                            TargetBalloon.SetActive(true);
                            TargetBalloon.transform.position = Ray.transform.GetComponent<BubbleScript>().GetBubblePosition(Ray.transform.GetComponent<BubbleScript>().GetBubbleDirection(Ray.point));
                            BubbleHit = true;
                        }
                        else
                        {
                            TargetingLine.enabled = false;
                            TargetBalloon.SetActive(false);
                            BubbleHit = false;
                        }
                    }
                    else
                    {
                        origin = Ray.point;
                        direction.x = direction.x * -1;
                        origin += direction;
                        Ray = Physics2D.Raycast(origin, direction, Mathf.Infinity);
                        if (Ray.transform != null && Ray.transform.tag == "Bubble")
                        {
                            if (Ray.transform.GetComponent<BubbleScript>().GetBubbleDirection(Ray.point) != BubbleScript.NeighborDirection.NONE)
                            {
                                TargetLine.Add(new Vector3(Ray.point.x, Ray.point.y, TargetingLine.transform.position.z));
                                TargetingLine.positionCount = TargetLine.Count;
                                TargetingLine.SetPositions(TargetLine.ToArray());
                                TargetingLine.enabled = true;
                                TargetBalloon.SetActive(true);
                                TargetBalloon.transform.position = Ray.transform.GetComponent<BubbleScript>().GetBubblePosition(Ray.transform.GetComponent<BubbleScript>().GetBubbleDirection(Ray.point));
                                BubbleHit = true;
                            }
                            else
                            {
                                TargetingLine.enabled = false;
                                TargetBalloon.SetActive(false);
                                BubbleHit = false;
                            }
                        }
                        else
                        {
                            TargetingLine.enabled = false;
                            TargetBalloon.SetActive(false);
                            BubbleHit = false;
                        }
                    }
                }
            }
            else if (d.phase == TouchPhase.Canceled || d.phase == TouchPhase.Ended)
            {
                if (BubbleHit)
                {
                    TargetLine[0] = CurrentBubble.transform.position;
                    TargetLine[TargetLine.Count - 1] = TargetBalloon.transform.position;
                    TargetBalloon.SetActive(false);
                    TargetingLine.enabled = false;
                    currentIndex = 0;

                    if (CurrentBubble.GetComponent<TrailRenderer>() != null)
                        CurrentBubble.GetComponent<TrailRenderer>().enabled = true;

                    currentState = ShootState.ShotMoving;
                }
            }
        }
    }

    void SpawnBubbles()
    {
        SetHeights();
        if (CurrentRows < 3)
        {
            while (CurrentRows < 3)
            {
                var Bubbles = BubbleHolder.GetComponentsInChildren<BubbleScript>();

                for (int i = 0; i < Bubbles.Length; i++)
                {
                    Vector3 newpos = Bubbles[i].transform.position;
                    newpos.y = newpos.y - yOffset;
                    Bubbles[i].transform.position = newpos;
                    Bubbles[i].StepHeight++;
                }

                if (OddRow)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Vector3 NewPosition = TopLeftCornerPivot.position;
                        NewPosition.x = NewPosition.x + (i * xOffset);
                        var bubble = Instantiate(BubblePrefab, NewPosition, Quaternion.identity, BubbleHolder.transform).GetComponent<BubbleScript>();
                        bubble.SetRandomValue(BubbleScript.BubbleValue.Two, BubbleScript.BubbleValue.SixtyFour);
                        bubble.StepHeight = 0;
                    }

                    CurrentRows++;
                    OddRow = !OddRow;
                }
                else
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Vector3 NewPosition = TopLeftCornerPivot.position;
                        NewPosition.x = NewPosition.x + (xOffset / 2) + (i * xOffset);
                        var bubble = Instantiate(BubblePrefab, NewPosition, Quaternion.identity, BubbleHolder.transform).GetComponent<BubbleScript>();
                        bubble.SetRandomValue(BubbleScript.BubbleValue.Two, BubbleScript.BubbleValue.SixtyFour);
                        bubble.StepHeight = 0;
                    }
                    CurrentRows++;
                    OddRow = !OddRow;
                }
            }
        }
        else if (CurrentRows < 5)
        {
            var Bubbles = BubbleHolder.GetComponentsInChildren<BubbleScript>();

            for (int i = 0; i < Bubbles.Length; i++)
            {
                Vector3 newpos = Bubbles[i].transform.position;
                newpos.y = newpos.y - yOffset;
                Bubbles[i].transform.position = newpos;
                Bubbles[i].StepHeight++;
            }

            if (OddRow)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector3 NewPosition = TopLeftCornerPivot.position;
                    NewPosition.x = NewPosition.x + (i * xOffset);
                    var bubble = Instantiate(BubblePrefab, NewPosition, Quaternion.identity, BubbleHolder.transform).GetComponent<BubbleScript>();
                    bubble.SetRandomValue(BubbleScript.BubbleValue.Two, BubbleScript.BubbleValue.SixtyFour);
                    bubble.StepHeight = 0;
                }

                CurrentRows++;
                OddRow = !OddRow;
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector3 NewPosition = TopLeftCornerPivot.position;
                    NewPosition.x = NewPosition.x + (xOffset / 2) + (i * xOffset);
                    var bubble = Instantiate(BubblePrefab, NewPosition, Quaternion.identity, BubbleHolder.transform).GetComponent<BubbleScript>();
                    bubble.SetRandomValue(BubbleScript.BubbleValue.Two, BubbleScript.BubbleValue.SixtyFour);
                    bubble.StepHeight = 0;
                }
                CurrentRows++;
                OddRow = !OddRow;
            }
        }
        else if (CurrentRows > 6)
        {
            while (CurrentRows > 6)
            {
                var Bubbles = BubbleHolder.GetComponentsInChildren<BubbleScript>();

                for (int i = 0; i < Bubbles.Length; i++)
                {
                    if (Bubbles[i].StepHeight == 0)
                        Bubbles[i].Pop();
                }

                Bubbles = BubbleHolder.GetComponentsInChildren<BubbleScript>();

                for (int i = 0; i < Bubbles.Length; i++)
                {
                    Vector3 newpos = Bubbles[i].transform.position;
                    newpos.y = newpos.y + yOffset;
                    Bubbles[i].transform.position = newpos;
                    Bubbles[i].StepHeight--;
                }

                OddRow = !OddRow;
                CurrentRows--;
            }
        }

        currentState = ShootState.Reload;
    }

    BubbleScript BubbleToMergeInto;
    List<BubbleScript> BubblesToMove = new List<BubbleScript>();
    bool IsMovingToMergeBubble = false;

    void MergeBubbles()
    {
        if (!IsMovingToMergeBubble)
        {
            BubblesToMove.Clear();
            BubbleScript.BubbleValue targetValue = CurrentBubbleValue;
            BubblesToMove.Add(BubbleToMergeInto);
            bool FirstCheck = BubbleToMergeInto.GetNeighBorsOfvalue(targetValue, ref BubblesToMove);

            if (FirstCheck)
            {
                for (int i = 0; i < BubblesToMove.Count; i++)
                {
                    BubblesToMove[i].GetNeighBorsOfvalue(targetValue, ref BubblesToMove);
                }

                BubbleToMergeInto = BubblesToMove[BubblesToMove.Count - 1];

                if ((int)(BubbleToMergeInto.ThisValue) + BubblesToMove.Count >= (int)BubbleScript.BubbleValue.Count)
                {
                    BubblesToMove.Remove(BubbleToMergeInto);
                    BubblesToMove.TrimExcess();

                    IsMovingToMergeBubble = true;
                }
                else
                {
                    int NewNearbyMerges = 0;
                    for (int i = 0; i < BubblesToMove.Count; i++)
                    {
                        List<BubbleScript> valid = new List<BubbleScript>();
                        BubblesToMove[i].GetNeighBorsOfvalue((BubbleScript.BubbleValue)((int)(BubbleToMergeInto.ThisValue) + BubblesToMove.Count - 1), ref valid);

                        if (valid.Count > NewNearbyMerges)
                        {
                            NewNearbyMerges = valid.Count;
                            BubbleToMergeInto = BubblesToMove[i];
                        }

                    }
                    BubblesToMove.Remove(BubbleToMergeInto);
                    BubblesToMove.TrimExcess();

                    IsMovingToMergeBubble = true;
                }

            }
            else
            {
                currentState = ShootState.RemovingfloatingBubbles;
            }
        }
        else
        {
            bool allArrived = true;
            for (int i = 0; i < BubblesToMove.Count; i++)
            {
                if (!BubbleToMergeInto.transform.position.Equals(BubblesToMove[i].transform.position))
                {
                    allArrived = false;
                    BubblesToMove[i].transform.position = Vector3.MoveTowards(BubblesToMove[i].transform.position, BubbleToMergeInto.transform.position, BubbleSpeed * Time.deltaTime * 5);
                }
            }

            if (allArrived)
            {
                if ((int)(BubbleToMergeInto.ThisValue) + BubblesToMove.Count >= (int)BubbleScript.BubbleValue.Count)
                    BubbleToMergeInto.SetValue(BubbleScript.BubbleValue.TwoK);
                else
                    BubbleToMergeInto.SetValue((BubbleScript.BubbleValue)((int)BubbleToMergeInto.ThisValue + BubblesToMove.Count));

                if (BubbleToMergeInto.ThisValue == BubbleScript.BubbleValue.TwoK)
                {
                    for (int i = 0; i < BubblesToMove.Count; i++)
                    {
                        BubblesToMove[i].Pop();
                    }

                    SetNeighborBubbles();

                    BubbleToMergeInto.Explode();
                    currentState = ShootState.RemovingfloatingBubbles;
                    IsMovingToMergeBubble = false;
                }
                else
                {

                    for (int i = 0; i < BubblesToMove.Count; i++)
                    {
                        BubblesToMove[i].Pop();
                    }

                    BubblesToMove.Clear();

                    if (BubbleToMergeInto.GetNeighBorsOfvalue(BubbleToMergeInto.ThisValue, ref BubblesToMove))
                    {
                        IsMovingToMergeBubble = false;
                        CurrentBubbleValue = BubbleToMergeInto.ThisValue;
                    }
                    else
                    {
                        currentState = ShootState.RemovingfloatingBubbles;
                        IsMovingToMergeBubble = false;
                    }
                }
            }
        }
    }

    int currentIndex = 0;

    void BubbleMoving()
    {
        CurrentBubble.transform.position = Vector3.MoveTowards(CurrentBubble.transform.position, TargetLine[currentIndex], BubbleSpeed * Time.deltaTime * 5);

        if (CurrentBubble.transform.position.Equals(TargetLine[currentIndex]))
        {
            if (currentIndex == TargetLine.Count - 1)
            {
                BubbleToMergeInto = Instantiate(BubblePrefab, TargetLine[TargetLine.Count - 1], Quaternion.identity, BubbleHolder.transform).GetComponent<BubbleScript>();
                BubbleToMergeInto.SetValue(CurrentBubbleValue);
                CurrentBubble.transform.position = TargetLine[0];
                if (CurrentBubble.GetComponent<TrailRenderer>() != null)
                    CurrentBubble.GetComponent<TrailRenderer>().enabled = false;
                SetNeighborBubbles();
                currentState = ShootState.MergingBubbles;
                currentIndex = 0;
            }
            else
            {
                currentIndex++;
            }
        }
    }


    void DestroyFloating()
    {
        SetNeighborBubbles();
        SetHeights();

        var Bubbles = BubbleHolder.GetComponentsInChildren<BubbleScript>();

        List<BubbleScript> d = new List<BubbleScript>();

        for (int i = 0; i < Bubbles.Length; i++)
        {
            if (Bubbles[i].StepHeight == 0)
                d.Add(Bubbles[i]);
        }

        for (int i = 0; i < d.Count; i++)
        {
            d[i].GetAllNeighbors(ref d);
        }


        for (int i = 0; i < Bubbles.Length; i++)
        {
            if (!d.Contains(Bubbles[i]))
                Bubbles[i].Pop();
        }

        currentState = ShootState.AddingMoreBubbles;
    }

    void SetNeighborBubbles()
    {
        var Bubbles = BubbleHolder.GetComponentsInChildren<BubbleScript>();

        for (int i = 0; i < Bubbles.Length; i++)
        {
            Bubbles[i].GrabNeighborSpheres();
        }
    }


    void SetHeights()
    {
        var Bubbles = BubbleHolder.GetComponentsInChildren<BubbleScript>();

        int x = 0;

        for (int i = 0; i < Bubbles.Length; i++)
        {
            int y = (int)Mathf.Abs(TopLeftCornerPivot.position.y - Bubbles[i].transform.position.y);
            Bubbles[i].StepHeight = (int)(y / yOffset);

            if (i == 0 || x < Bubbles[i].StepHeight)
                x = Bubbles[i].StepHeight;
        }

        CurrentRows = x;
    }
}
