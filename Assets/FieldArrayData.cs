using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FieldArrayData : MonoBehaviour
{
    // タグリストの名前に紐づく番号
    const int NO_BLOCK = 0;
    const int STATIC_BLOCK = 1;
    const int MOVE_BLOCK = 2;
    const int PLAYER = 3;
    const int TARGET = 4;
    private int SetsceneIndex;
    [SerializeField] private Text text;
    /// <summary>
    /// シーンに配置するオブジェクトのルートをヒエラルキーから設定する
    /// </summary>
    [Header("配置するオブジェクトの親オブジェクトを設定")]
    [SerializeField] GameObject g_fieldRootObject;
    /// <summary>
    /// フィールドのオブジェクトリスト
    /// 0 空欄
    /// 1 動かないブロック
    /// 2 動くブロック
    /// 3 プレイヤー
    /// </summary>
    string[] g_fieldObjectTagList = {
"","StaticBlock","MoveBlock","Player","TargetPosition"
};
    [Header("動かないオブジェクトを設定(Tagを識別する)")]
    [SerializeField] GameObject g_staticBlock;
    [Header("動くオブジェクトを設定(Tagを識別する)")]
    [SerializeField] GameObject g_moveBlock;
    [Header("プレイヤーオブジェクトを設定(Tagを識別する)")]
    [SerializeField] GameObject g_player;
    [Header("ターゲットオブジェクトを設定(tagを識別する)")]
    [SerializeField] GameObject g_target;
    /// <summary>
    /// フィールドデータ用の変数を定義
    /// </summary>
    int[,] g_fieldData = {
{ 0, 0, 0, 0, 0, 0},
{ 0, 0, 0, 0, 0, 0},
{ 0, 0, 0, 0, 0, 0},
{ 0, 0, 0, 0, 0, 0},
{ 0, 0, 0, 0, 0, 0},
{ 0, 0, 0, 0, 0, 0},
};
    // 縦横の最大数
    int g_horizontalMaxCount = 0;
    int g_verticalMaxCount = 0;

    /// <summary>
    /// プレイヤーの位置情報
    /// </summary>
    public Vector2 PlayerPosition { get; set; }
    int[,] g_targetData = {
 { 0, 0, 0, 0, 0, 0 },
 { 0, 0, 0, 0, 0, 0 },
 { 0, 0, 0, 0, 0, 0 },
 { 0, 0, 0, 0, 0, 0 },
 { 0, 0, 0, 0, 0, 0 },
 { 0, 0, 0, 0, 0, 0 },
 };
    // ブロックがターゲットに入った数
    int g_targetClearCount = 0;
    // ターゲットの最大数
    int g_targetMaxCount = 0;
    /// <summary>
    /// fieldRootObjectの配下にあるオブジェクトのタグを読み取り
    /// XとY座標を基にfieldDataへ格納します(fieldDataは上書き削除します)
    /// fieldDataはfieldData[Y,X]で紐づいている
    /// フィールド初期化に使う
    /// </summary>
    /// <param name="fieldRootObject">フィールドオブジェクトのルートオブジェクトを設定</param>
    public void ImageToArray()
    {
        // フィールドの縦と横の最大数を取得(フィールドの大きさを取得)
        foreach (Transform fieldObject in g_fieldRootObject.transform)
        {
            /*
            * 縦方向に関しては座標の兼ね合い上
            * 下に行くほどyは減っていくので-をつけることで
            * yの位置を逆転させている
            */
            int col = Mathf.FloorToInt(fieldObject.position.x);
            int row = Mathf.FloorToInt(-fieldObject.position.y);
            if (g_fieldObjectTagList[STATIC_BLOCK].Equals(fieldObject.tag))
            {
                g_fieldData[row, col] = STATIC_BLOCK;
            }
            else if (g_fieldObjectTagList[MOVE_BLOCK].Equals(fieldObject.tag))
            {
                g_fieldData[row, col] = MOVE_BLOCK;
            }
            else if (g_fieldObjectTagList[PLAYER].Equals(fieldObject.tag))
            {
                g_fieldData[row, col] = PLAYER;
                PlayerPosition = new Vector2(row, col);
            }
            else if (g_fieldObjectTagList[TARGET].Equals(fieldObject.tag))
            {
                g_fieldData[row, col] = TARGET;
                // ターゲットの最大カウント
                g_targetMaxCount++;
            }
            // フィールドデータをターゲット用のデータにコピーする
            g_targetData = (int[,])g_fieldData.Clone();
        }
    }
    /// <summary>
    /// フィールドのサイズを設定する
    /// フィールドの初期化に使う
    /// </summary>
    public void SetFieldMaxSize()
    {
        // フィールドの縦と横の最大数を取得(フィールドの大きさを取得)
        foreach (Transform fieldObject in g_fieldRootObject.transform)
        {
            /*
            * 縦方向に関しては座標の兼ね合い上
            * 下に行くほどyは減っていくので-をつけることで
            * yの位置を逆転させている
            */
            int positionX = Mathf.FloorToInt(fieldObject.position.x);
            int positionY = Mathf.FloorToInt(-fieldObject.position.y);
            // 横の最大数を設定する
            if (g_horizontalMaxCount < positionX)
            {
                g_horizontalMaxCount = positionX;
            }
            // 縦の最大数を設定する
            if (g_verticalMaxCount < positionY)
            {
                g_verticalMaxCount = positionY;
            }
        }
        // フィールド配列の初期化
        g_fieldData = new int[g_verticalMaxCount + 1, g_horizontalMaxCount + 1];
    }
    /// <summary>
    /// 初回起動時
    /// シーンに配置されたオブジェクトを元に配列データを生成する
    /// </summary>
    private void Awake()
    {
        SetFieldMaxSize();
        ImageToArray();
        g_fieldArrayData = GetComponent<FieldArrayData>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            // 配列を出力するテスト
            print("Field------------------------------------------");
            for (int y = 0; y <= g_verticalMaxCount; y++)
            {
                string outPutString = "";
                for (int x = 0; x <= g_horizontalMaxCount; x++)
                {
                    outPutString += g_fieldData[y, x];
                }
                print(outPutString);
            }
            print("Field------------------------------------------");
            print("プレイヤーポジション:" + PlayerPosition);
        }
        //リセットボタン
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetsceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(SetsceneIndex);
        }
        // ゲーム状態によって処理を分ける
        switch (g_gameState)
        {
            case GameState.START:
                SetGameState(GameState.PLAYER);
                break;
            case GameState.STOP:
                break;
            case GameState.PLAYER:
                float horizontalInput = Input.GetAxisRaw("Horizontal");
                float verticalInput = Input.GetAxisRaw("Vertical");
                // 横入力が0より大きい場合は右に移動
                if (horizontalInput > 0 && !g_inputState)
                {
                    g_fieldArrayData.PlayerMove(
                    Mathf.FloorToInt(g_fieldArrayData.PlayerPosition.x),
                    Mathf.FloorToInt(g_fieldArrayData.PlayerPosition.y),
                    Mathf.FloorToInt(g_fieldArrayData.PlayerPosition.x),
                    Mathf.FloorToInt(g_fieldArrayData.PlayerPosition.y + 1));
                    g_inputState = true;
                }
                // 横入力が0より小さい場合は左に移動
                else if (horizontalInput < 0 && !g_inputState)
                {
                    g_fieldArrayData.PlayerMove(
                    Mathf.FloorToInt(g_fieldArrayData.PlayerPosition.x),
                    Mathf.FloorToInt(g_fieldArrayData.PlayerPosition.y),
                    Mathf.FloorToInt(g_fieldArrayData.PlayerPosition.x),
                    Mathf.FloorToInt(g_fieldArrayData.PlayerPosition.y - 1));
                    g_inputState = true;
                }
                // 縦入力が0より大きい場合は上に移動
                if (verticalInput > 0 && !g_inputState)
                {
                    g_fieldArrayData.PlayerMove(
                    Mathf.FloorToInt(g_fieldArrayData.PlayerPosition.x),
                    Mathf.FloorToInt(g_fieldArrayData.PlayerPosition.y),
                    Mathf.FloorToInt(g_fieldArrayData.PlayerPosition.x - 1),
                    Mathf.FloorToInt(g_fieldArrayData.PlayerPosition.y));
                    g_inputState = true;
                }
                // 縦入力が0より小さい場合は下に移動
                else if (verticalInput < 0 && !g_inputState)
                {
                    g_fieldArrayData.PlayerMove(
                    Mathf.FloorToInt(g_fieldArrayData.PlayerPosition.x),
                    Mathf.FloorToInt(g_fieldArrayData.PlayerPosition.y),
                    Mathf.FloorToInt(g_fieldArrayData.PlayerPosition.x + 1),
                    Mathf.FloorToInt(g_fieldArrayData.PlayerPosition.y));
                    g_inputState = true;
                }
                // 入力状態が解除されるまで再入力できないようにする
                if ((horizontalInput + verticalInput) == 0)
                {
                    g_inputState = false;
                }
                //クリア判定
                if (g_fieldArrayData.GetGameClearJudgment())
                {
                    g_gameState = GameState.END;
                }
                break;
            case GameState.BLOCK_MOVE:
                break;
            case GameState.END:
                break;
        }
    }
    public GameObject GetFieldObject(int tagId, int row, int col)
    {
        foreach(Transform fieldObject in g_fieldRootObject.transform)
        {
            if(tagId != -1 && fieldObject.tag 
                != g_fieldObjectTagList[tagId])
            {
                continue;
            }
            if(fieldObject.transform.position.x == col && 
                fieldObject.transform.position.y == -row)
            {
                return fieldObject.gameObject;
            }
        }
        return null;
    }
    public void MoveData(int preRow, int preCol, int nextRow, int nextCol)
    {
        GameObject moveObject = GetFieldObject(g_fieldData[preRow, preCol], preRow, preCol);
        if (moveObject != null)
        {
            moveObject.transform.position = new Vector2(nextCol, -nextRow);
        }
        g_fieldData[nextRow, nextCol] = g_fieldData[preRow, preCol];
        g_fieldData[preRow, preCol] = NO_BLOCK;
    }
    public bool BlockMoveCheck(int y, int x)
    {
        // ターゲットブロックだったら
        if(g_targetData[y,x] == TARGET)
        {
            // ターゲットクリアカウントを上げる
            g_targetClearCount++;
            return true;
        }
        return g_fieldData[y, x] == NO_BLOCK;
    }
    public bool BlockMove(int preRow, int preCol, int nextRow, int nextCol)
    {
        // 境界線外エラー
        if (nextRow < 0 || nextCol < 0 ||
        nextRow > g_verticalMaxCount || nextCol > g_horizontalMaxCount)
        {
            return false;
        }
        bool moveFlag = BlockMoveCheck(nextRow, nextCol);
        // 移動可能かチェックする
        if (moveFlag)
        {
            // 移動が可能な場合移動する
            MoveData(preRow, preCol, nextRow, nextCol);
        }
        return moveFlag;
    }
    public bool PlayerMoveCheck(int preRow, int preCol, int nextRow, int nextCol)
    {
        /* プレイヤーの移動先が動くブロックの時
        * ブロックを移動する処理を実施する
        */
        if (g_fieldData[nextRow, nextCol] == MOVE_BLOCK)
        {
            bool blockMoveFlag = BlockMove(nextRow, nextCol,
            nextRow + (nextRow - preRow),
            nextCol + (nextCol - preCol));
            return blockMoveFlag;
        }
        // プレイヤーの移動先が空の時移動する
        // プレイヤーの移動先がターゲットの時移動する
        if (g_fieldData[nextRow, nextCol] == NO_BLOCK || 
            g_fieldData[nextRow, nextCol] == TARGET)
        {
            return true;
        }
        return false;
    }
    public void PlayerMove(int preRow, int preCol, int nextRow, int nextCol)
    {
        // 移動可能かチェックする
        if (PlayerMoveCheck(preRow, preCol, nextRow, nextCol))
        {
            // 移動が可能な場合移動する
            MoveData(preRow, preCol, nextRow, nextCol);
            // プレイヤーの位置を更新する
            // 座標情報なので最初の引数はX
            PlayerPosition = new Vector2(nextRow, nextCol);
        }
    }
    FieldArrayData g_fieldArrayData;
    enum GameState
    {
        START, STOP, BLOCK_MOVE, PLAYER, END,
    }
    [SerializeField] GameState g_gameState = GameState.START;
    void SetGameState(GameState gameState)
    {
        this.g_gameState = gameState;
    }
    GameState GetGameState()
    {
        return this.g_gameState;
    }
    bool g_inputState = false;
    public bool GetGameClearJudgment()
    {
        // ターゲットクリア数とターゲットの最大数が一致したらゲームクリア
        if(g_targetClearCount == g_targetMaxCount)
        {
            text.gameObject.SetActive(true);
            print("ゲームクリア!");
            return true;
        }
        return false;
    }
}