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
    //現在のシーンが何番目にあるか
    private int SetsceneIndex;
    //ゲームクリア時のテキストと次のステージ移動のテキスト
    [Header("ゲームクリア時のテキストを設定")]
    [SerializeField] private Text _clearText;
    [Header("次のステージ移動のテキストを設定")]
    [SerializeField] private Text _text;
    //現在のスコアのテキスト
    public GameObject score_object = null;
    //スコア変数
    public int score_num = 0;
    /// <summary>
    /// シーンに配置するオブジェクトのルートをヒエラルキーから設定する
    /// </summary>
    [Header("配置するオブジェクトの親オブジェクトを設定")]
    [SerializeField] GameObject _fieldRootObject = default;
    /// <summary>
    /// フィールドのオブジェクトリスト
    /// 0 空欄
    /// 1 動かないブロック
    /// 2 動くブロック
    /// 3 プレイヤー
    /// </summary>
    string[] _fieldObjectTagList = {
        "","StaticBlock","MoveBlock","Player","TargetPosition"
    };
    [Header("動かないオブジェクトを設定(Tagを識別する)")]
    [SerializeField] GameObject _staticBlock = default;
    [Header("動くオブジェクトを設定(Tagを識別する)")]
    [SerializeField] GameObject _moveBlock = default;
    [Header("プレイヤーオブジェクトを設定(Tagを識別する)")]
    [SerializeField] GameObject _player = default;
    [Header("ターゲットオブジェクトを設定(tagを識別する)")]
    [SerializeField] GameObject _target = default;
    /// <summary>
    /// フィールドデータ用の変数を定義
    /// </summary>
    int[,] _fieldData = {
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0},
    };
    // 縦横の最大数
    int _horizontalMaxCount = 0;
    int _verticalMaxCount = 0;

    /// <summary>
    /// プレイヤーの位置情報
    /// </summary>
    public Vector2 PlayerPosition { get; set; }
    int[,] _targetData = {
        { 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0 },
     };
    // ブロックがターゲットに入った数
    int _targetClearCount = 0;
    // ターゲットの最大数
    int _targetMaxCount = 0;
    /// <summary>
    /// fieldRootObjectの配下にあるオブジェクトのタグを読み取り
    /// XとY座標を基にfieldDataへ格納します(fieldDataは上書き削除します)
    /// fieldDataはfieldData[Y,X]で紐づいている
    /// フィールド初期化に使う
    /// </summary>
    /// <param name="fieldRootObject">フィールドオブジェクトのルートオブジェクトを設定</param>]
    bool _inputState = false;
    public void ImageToArray()
    {
        // フィールドの縦と横の最大数を取得(フィールドの大きさを取得)
        foreach (Transform fieldObject in _fieldRootObject.transform)
        {
            /*
            * 縦方向に関しては座標の兼ね合い上
            * 下に行くほどyは減っていくので-をつけることで
            * yの位置を逆転させている
            */
            int col = Mathf.FloorToInt(fieldObject.position.x);
            int row = Mathf.FloorToInt(-fieldObject.position.y);
            if (_fieldObjectTagList[STATIC_BLOCK].Equals(fieldObject.tag))
            {
                _fieldData[row, col] = STATIC_BLOCK;
            }
            else if (_fieldObjectTagList[MOVE_BLOCK].Equals(fieldObject.tag))
            {
                _fieldData[row, col] = MOVE_BLOCK;
            }
            else if (_fieldObjectTagList[PLAYER].Equals(fieldObject.tag))
            {
                _fieldData[row, col] = PLAYER;
                PlayerPosition = new Vector2(row, col);
            }
            else if (_fieldObjectTagList[TARGET].Equals(fieldObject.tag))
            {
                _fieldData[row, col] = TARGET;
                // ターゲットの最大カウント
                _targetMaxCount++;
            }
            // フィールドデータをターゲット用のデータにコピーする
            _targetData = (int[,])_fieldData.Clone();
        }
    }
    /// <summary>
    /// フィールドのサイズを設定する
    /// フィールドの初期化に使う
    /// </summary>
    public void SetFieldMaxSize()
    {
        // フィールドの縦と横の最大数を取得(フィールドの大きさを取得)
        foreach (Transform fieldObject in _fieldRootObject.transform)
        {
            /*
            * 縦方向に関しては座標の兼ね合い上
            * 下に行くほどyは減っていくので-をつけることで
            * yの位置を逆転させている
            */
            int positionX = Mathf.FloorToInt(fieldObject.position.x);
            int positionY = Mathf.FloorToInt(-fieldObject.position.y);
            // 横の最大数を設定する
            if (_horizontalMaxCount < positionX)
            {
                _horizontalMaxCount = positionX;
            }
            // 縦の最大数を設定する
            if (_verticalMaxCount < positionY)
            {
                _verticalMaxCount = positionY;
            }
        }
        // フィールド配列の初期化
        _fieldData = new int[_verticalMaxCount + 1, _horizontalMaxCount + 1];
    }
    /// <summary>
    /// 初回起動時
    /// シーンに配置されたオブジェクトを元に配列データを生成する
    /// </summary>
    private void Awake()
    {
        SetFieldMaxSize();
        ImageToArray();
        _fieldArrayData = GetComponent<FieldArrayData>();
    }
    private void Update()
    {
        //オブジェクトからTextコンポーネントを取得
        Text score_text = score_object.GetComponent<Text>();

        if (Input.GetKeyDown(KeyCode.H))
        {
            // 配列を出力するテスト
            print("Field------------------------------------------");
            for (int y = 0; y <= _verticalMaxCount; y++)
            {
                string outPutString = "";
                for (int x = 0; x <= _horizontalMaxCount; x++)
                {
                    outPutString += _fieldData[y, x];
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
        switch (_gameState)
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
                if (horizontalInput > 0 && !_inputState)
                {
                    _fieldArrayData.PlayerMove(
                    Mathf.FloorToInt(_fieldArrayData.PlayerPosition.x),
                    Mathf.FloorToInt(_fieldArrayData.PlayerPosition.y),
                    Mathf.FloorToInt(_fieldArrayData.PlayerPosition.x),
                    Mathf.FloorToInt(_fieldArrayData.PlayerPosition.y + 1));
                    _inputState = true;
                }
                // 横入力が0より小さい場合は左に移動
                else if (horizontalInput < 0 && !_inputState)
                {
                    _fieldArrayData.PlayerMove(
                    Mathf.FloorToInt(_fieldArrayData.PlayerPosition.x),
                    Mathf.FloorToInt(_fieldArrayData.PlayerPosition.y),
                    Mathf.FloorToInt(_fieldArrayData.PlayerPosition.x),
                    Mathf.FloorToInt(_fieldArrayData.PlayerPosition.y - 1));
                    _inputState = true;
                }
                // 縦入力が0より大きい場合は上に移動
                if (verticalInput > 0 && !_inputState)
                {
                    _fieldArrayData.PlayerMove(
                    Mathf.FloorToInt(_fieldArrayData.PlayerPosition.x),
                    Mathf.FloorToInt(_fieldArrayData.PlayerPosition.y),
                    Mathf.FloorToInt(_fieldArrayData.PlayerPosition.x - 1),
                    Mathf.FloorToInt(_fieldArrayData.PlayerPosition.y));
                    _inputState = true;
                }
                // 縦入力が0より小さい場合は下に移動
                else if (verticalInput < 0 && !_inputState)
                {
                    _fieldArrayData.PlayerMove(
                    Mathf.FloorToInt(_fieldArrayData.PlayerPosition.x),
                    Mathf.FloorToInt(_fieldArrayData.PlayerPosition.y),
                    Mathf.FloorToInt(_fieldArrayData.PlayerPosition.x + 1),
                    Mathf.FloorToInt(_fieldArrayData.PlayerPosition.y));
                    _inputState = true;
                }
                // 入力状態が解除されるまで再入力できないようにする
                if ((horizontalInput + verticalInput) == 0)
                {
                    _inputState = false;
                }
                //クリア判定
                if (_fieldArrayData.GetGameClearJudgment())
                {
                    _gameState = GameState.END;
                }
                break;
            case GameState.BLOCK_MOVE:
                break;
            case GameState.END:
                break;
        }
        score_text.text = "" + score_num;
    }
    public GameObject GetFieldObject(int tagId, int row, int col)
    {
        foreach(Transform fieldObject in _fieldRootObject.transform)
        {
            if(tagId != -1 && fieldObject.tag 
                != _fieldObjectTagList[tagId])
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
        GameObject moveObject = GetFieldObject(_fieldData[preRow, preCol], preRow, preCol);
        if (moveObject != null)
        {
            moveObject.transform.position = new Vector2(nextCol, -nextRow);
        }
        _fieldData[nextRow, nextCol] = _fieldData[preRow, preCol];
        _fieldData[preRow, preCol] = NO_BLOCK;
    }
    public bool BlockMoveCheck(int y, int x)
    {
        // ターゲットブロックだったら
        if(_targetData[y,x] == TARGET)
        {
            // ターゲットクリアカウントを上げる
            _targetClearCount++;
            return true;
        }
        return _fieldData[y, x] == NO_BLOCK;
    }
    public bool BlockMove(int preRow, int preCol, int nextRow, int nextCol)
    {
        // 境界線外エラー
        if (nextRow < 0 || nextCol < 0 ||
        nextRow > _verticalMaxCount || nextCol > _horizontalMaxCount)
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
        if (_fieldData[nextRow, nextCol] == MOVE_BLOCK)
        {
            bool blockMoveFlag = BlockMove(nextRow, nextCol,
            nextRow + (nextRow - preRow),
            nextCol + (nextCol - preCol));
            return blockMoveFlag;
        }
        // プレイヤーの移動先が空の時移動する
        // プレイヤーの移動先がターゲットの時移動する
        if (_fieldData[nextRow, nextCol] == NO_BLOCK || 
            _fieldData[nextRow, nextCol] == TARGET)
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
            score_num += 1;
        }
    }
    FieldArrayData _fieldArrayData;
    enum GameState
    {
        START, STOP, BLOCK_MOVE, PLAYER, END,
    }
    [SerializeField] GameState _gameState = GameState.START;
    void SetGameState(GameState gameState)
    {
        this._gameState = gameState;
    }
    GameState GetGameState()
    {
        return this._gameState;
    }
    public bool GetGameClearJudgment()
    {
        // ターゲットクリア数とターゲットの最大数が一致したらゲームクリア
        // クリア時に“ゲームクリア”のテキスト表示と“NextStage : EnterKey”を表示
        if(_targetClearCount == _targetMaxCount)
        {
            _text.gameObject.SetActive(true);
            _clearText.gameObject.SetActive(true);
            print("ゲームクリア!");
            return true;
        }
        return false;
    }
}