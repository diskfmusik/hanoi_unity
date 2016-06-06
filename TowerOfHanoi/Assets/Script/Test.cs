using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Test : MonoBehaviour
{


    const int Max = 3;


    struct PoleInfo
    {
        public string name;
        public int[] ary;

        public void SetName(string _name)
        {
            name = _name;
            ary = new int[Max];
        }

        public int GetSize()
        {
            int num = 0;

            for (int i = 0; i < Max; i++)
            {
                if (ary[i] > 0) ++num;
            }
            return num;
        }
    };


    struct MoveInfo
    {
        //public string fromStr, toStr;
        public int fromElementNum, toElementNum;
        public int diskNumber;
        public float afterX, afterY;
        public float beforeX, beforeY;
    };


    // disk[0] : 小
    // disk[1] : 中
    // disk[2] : 大
    [SerializeField]
    GameObject[] disk = new GameObject[Max];


    // disk を置く座標
    float[] x = { -6, 0, 6 };
    float[] y = { 0, 0.5f, 1.0f };
    const float upY = 5.0f;


    // 計算用のデータ
    PoleInfo poleA;
    PoleInfo poleB;
    PoleInfo poleC;
    MoveInfo moveInfo;


    // debug用
    [SerializeField]
    Text text;

    [SerializeField]
    Text countText;


    void Start()
    {
        poleA.SetName("A");
        poleB.SetName("B");
        poleC.SetName("C");

        // 初期化
        for (int i = 0; i < Max; i++)
        {
            poleA.ary[i] = Max - i;
            poleB.ary[i] = 0;
            poleC.ary[i] = 0;

            DiskInfo info = disk[i].GetComponent<DiskInfo>();
            info.aryNumber = i + 1;
            info.elementNumber = (Max - 1) - i;

            disk[i].gameObject.transform.localPosition = new Vector3(x[0], y[(Max - 1) - i], 0);
        }
        Print();

        //StartCoroutine(MoveCalc(a, c, b, Max));
        StartCoroutine(MoveCalc(poleA, poleC, poleB, Max));
    }


    void Print()
    {
        string str = "\n";
        for (int i = Max - 1; i >= 0; i--)
        {
            str += poleA.ary[i];
            str += poleB.ary[i];
            str += poleC.ary[i] + "\n";
        }

        //Debug.Log(str);
        text.text = str;
    }


    void Swap(PoleInfo _from, PoleInfo _to)
    {
        int fromSize = _from.GetSize();
        --fromSize;

        int toSize = _to.GetSize();


        //------
        // ToDo
        //------
        // _from のデータから、動かす Disk の確定
        // 何処に動かすか？

        //moveInfo.fromStr = _from.name;
        //moveInfo.toStr = _to.name;
        moveInfo.fromElementNum = fromSize;
        moveInfo.toElementNum = toSize;

        moveInfo.afterX = GetMoveAfterX(_to.name);
        moveInfo.afterY = y[moveInfo.toElementNum];

        moveInfo.diskNumber = GetDiskNum(_from.name);

        Vector3 vec = disk[moveInfo.diskNumber].gameObject.transform.localPosition;
        moveInfo.beforeX = vec.x;
        moveInfo.beforeY = vec.y;

        //------

        int temp = _from.ary[fromSize];
        _from.ary[fromSize] = _to.ary[toSize];
        _to.ary[toSize] = temp;
    }


    float GetMoveAfterX(string _name)
    {
        float f = 0;

        switch (_name)
        {
            case "A":
                f = x[0];
                break;
            case "B":
                f = x[1];
                break;
            case "C":
                f = x[2];
                break;
        }

        return f;
    }


    // 値を swap する前に、動かす Disk を確定する
    int GetDiskNum(string _fromStr)
    {
        int num = 0;

        for (int i = 0; i < Max; i++)
        {
            DiskInfo info = disk[i].GetComponent<DiskInfo>();

            // (計算用の) ary から値を取ってきて、
            // diskInfo の aryNumber と 比較して、
            switch (_fromStr)
            {
                case "A":
                    if (poleA.ary[moveInfo.fromElementNum] == info.aryNumber) num = i;
                    break;
                case "B":
                    if (poleB.ary[moveInfo.fromElementNum] == info.aryNumber) num = i;
                    break;
                case "C":
                    if (poleC.ary[moveInfo.fromElementNum] == info.aryNumber) num = i;
                    break;
            }
        }

        return num;
    }




    IEnumerator MoveCalc(PoleInfo _from, PoleInfo _to, PoleInfo _work, int _n)
    {
        if (_n < 1)
        {
            //return;
            yield return new WaitForSeconds(0);
        }
        else if (_n == 1)
        {
            Swap(_from, _to);
            Print();
            yield return StartCoroutine(WaitToFinishAnimation()); // アニメーション完了するまで待機
        }
        else
        {
            yield return StartCoroutine(MoveCalc(_from, _work, _to, _n - 1));

            Swap(_from, _to);
            Print();
            yield return StartCoroutine(WaitToFinishAnimation()); // アニメーション完了するまで待機

            yield return StartCoroutine(MoveCalc(_work, _to, _from, _n - 1));
        }
    }




    bool isAnimationEnd = false;
    bool isNeedsAnimation = false;
    bool isHanoiAnimationEnd = false;


    IEnumerator WaitToFinishAnimation()
    {
        //disk[moveInfo.diskNumber].gameObject.transform.localPosition = new Vector3(x[moveInfo.moveAfterX], y[moveInfo.toElementNum], 0);


        // アニメーションを開始させる
        isNeedsAnimation = true;

        // アニメーション完了フラグが立つまで、先に進まない
        while (!isAnimationEnd)
        {
            yield return null; // 次のフレームまで待機
        }

        // アニメーションが完了したので、
        // フラグのリセット
        isAnimationEnd = false;
        isNeedsAnimation = false;
    }




    int frame = 0;
    void Update()
    {
        if (isHanoiAnimationEnd) return;

        //---------------------------------------------------------------------

        if (isNeedsAnimation) ++frame;


        // 0 ~ 20 : 上に持っていく
        if (frame <= 20)
        {
            float plus = (upY - moveInfo.beforeY) / 20.0f * frame;

            disk[moveInfo.diskNumber].gameObject.transform.localPosition = new Vector3(moveInfo.beforeX, moveInfo.beforeY + plus, 0);
        }
        // 20 ~ 40 : 左右の移動
        else if (frame <= 40)
        {
            float plus = (moveInfo.afterX - moveInfo.beforeX) / 20.0f * (frame - 20);

            disk[moveInfo.diskNumber].gameObject.transform.localPosition = new Vector3(moveInfo.beforeX + plus, upY, 0);
        }
        // 40 ~ 60 : 降ろす
        else if (frame <= 60)
        {
            float plus = (moveInfo.afterY - upY) / 20.0f * (frame - 40);

            disk[moveInfo.diskNumber].gameObject.transform.localPosition = new Vector3(moveInfo.afterX, upY + plus, 0);
        }


        // 60 フレーム経ったら
        // アニメーション完了フラグを立てる
        if (frame >= 60)
        {
            isAnimationEnd = true;
            frame = 0;

            // ミッション完了したら
            if (poleC.GetSize() >= Max) isHanoiAnimationEnd = true;
        }

        // debug用
        // frame表示
        countText.text = frame.ToString();
    }


} // public class Test : MonoBehaviour
