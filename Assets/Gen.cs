using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Gen : MonoBehaviour
{
    private List<GameObject> objects = new List<GameObject>();

    public RectTransform CWContainer;
    protected float CWContainerWidth;
    protected float CWContainerHeight;

    public RectTransform QContainer;
    protected float QContainerWidth;
    protected float QContainerHeight;
    protected float QHeight;
    protected int QColumns = 2;

    public Dictionary<string, string> List = new Dictionary<string, string>();
    public int MaxWordLength;

    public string[,] Grid = new string[40, 40];
    private bool empty = true;
    private int QCounter = 1;

    private float GridCellMargin = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        CWContainerWidth = CWContainer.rect.width;
        CWContainerHeight = CWContainer.rect.height;

        QContainerWidth = QContainer.rect.width;
        QContainerHeight = QContainer.rect.height;

        ReadInput();

        QHeight = 1f / List.Count * QColumns;

        DrawGrid();
    }

    private void ReadInput()
    {
        string[] lines = System.IO.File.ReadAllLines("Assets/cw.txt");
        foreach (string line in lines)
        {
            string[] elem = line.Split('|');
            List.Add(elem[1].Trim(), elem[0].Trim());
        }
        MaxWordLength = List.Max(x => x.Key.Length);
        Debug.Log(List.Count + " words read. Longest word is " + MaxWordLength);
    }

    private void DrawGrid()
    {
        Clear();
        int numCellsX = Grid.GetLength(0);
        int numCellsY = Grid.GetLength(1);

        float cellSize = 1f / (Math.Max(numCellsX, numCellsY));

        for(int y = 0; y < numCellsY; y++)
        {
            for(int x = 0; x < numCellsX ; x++)
            {
                string content = Grid[x, y];
                bool numeric = IsNumeric(content);
                bool block = content == "*";

                RectTransform cell = AddPanel("Cell " + x + "/" + y, numeric || block ? Color.grey : Color.white, x * cellSize + (cellSize *GridCellMargin), y * cellSize + (cellSize * GridCellMargin), (x + 1) * cellSize - (cellSize * GridCellMargin), (y + 1) * cellSize - (cellSize * GridCellMargin), CWContainer);
                AddText(Grid[x, y], 20, Color.black, FontStyle.Bold, 0, 0, 1, 1, cell);
            }
        }
    }

    private void AddWord()
    {
        if (empty)
        {
            empty = false;
            KeyValuePair<string, string> kvp = List.ElementAt(UnityEngine.Random.Range(0, List.Count));

            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                AddWordToGrid(new WordCandidate(0, 1, true, kvp, 0));
            }
            else
            {
                AddWordToGrid(new WordCandidate(1, 0, false, kvp, 0));
            }
        }

        else
        {
            List<WordCandidate> candidates = new List<WordCandidate>();

            int numCellsX = Grid.GetLength(0);
            int numCellsY = Grid.GetLength(1);

            for (int y = 0; y < numCellsY; y++)
            {
                for (int x = 0; x < numCellsX; x++)
                {
                    string content = Grid[x, y];
                    bool possibleStart = (content == "" || content == null || content == "*");
                    if(possibleStart)
                    {
                        // Check right
                        int rightLength = 0;
                        int tmpX = x + 1;
                        Dictionary<int, char> lettersX = new Dictionary<int, char>(); // Key is position in word, value is letter
                        while(tmpX < numCellsX && !IsNumeric(Grid[tmpX,y]) && Grid[tmpX,y] != "*" && rightLength < MaxWordLength)
                        {
                            if (Grid[tmpX, y] != null && Grid[tmpX, y] != "") lettersX.Add(rightLength, Grid[tmpX, y][0]);
                            tmpX++;
                            rightLength++;
                        }
                        if(rightLength > 2 && lettersX.Count > 0) // Possible start for horizontal word
                        {
                            // Look for words with letter requirements
                            List<KeyValuePair<string, string>> candQ = new List<KeyValuePair<string, string>>();
                            candQ.AddRange(List.Where(q => q.Key.Length < rightLength));
                            foreach(KeyValuePair<int, char> letter in lettersX)
                            {
                                List<KeyValuePair<string, string>> letterCand = List.Where(q => q.Key.Length > letter.Key && q.Key[letter.Key] == letter.Value).ToList();
                                candQ = candQ.Intersect(letterCand).ToList();
                            }

                            foreach(KeyValuePair<string, string> kvp in candQ)
                            {
                                candidates.Add(new WordCandidate(x, y, true, kvp, lettersX.Count));
                            }
                        }

                        // Check down
                        int downLength = 0;
                        int tmpY = y + 1;
                        Dictionary<int, char> lettersY = new Dictionary<int, char>(); // Key is position in word, value is letter
                        while (tmpY < numCellsY && !IsNumeric(Grid[x, tmpY]) && Grid[x, tmpY] != "*" && downLength < MaxWordLength)
                        {
                            if (Grid[x, tmpY] != null && Grid[x, tmpY] != "") lettersY.Add(downLength, Grid[x, tmpY][0]);
                            tmpY++;
                            downLength++;
                        }
                        if (downLength > 2 && lettersY.Count > 0) // Possible start for vertical word
                        {
                            // Look for words with letter requirements
                            List<KeyValuePair<string, string>> candQ = new List<KeyValuePair<string, string>>();
                            candQ.AddRange(List.Where(q => q.Key.Length < downLength));
                            foreach (KeyValuePair<int, char> letter in lettersY)
                            {
                                List<KeyValuePair<string, string>> letterCand = List.Where(q => q.Key.Length > letter.Key && q.Key[letter.Key] == letter.Value).ToList();
                                candQ = candQ.Intersect(letterCand).ToList();
                            }

                            foreach (KeyValuePair<string, string> kvp in candQ)
                            {
                                candidates.Add(new WordCandidate(x, y, false, kvp, lettersY.Count));
                            }
                        }
                    }
                }
            }

            /*
            Debug.Log("----------------------" + "Possible words for Question " + QCounter + "----------------------");
            foreach(WordCandidate wc in candidates)
            {
                Debug.Log(wc.X + "/" + wc.Y + " " + (wc.Horizontal ? "Hor: " : " Ver: ") + wc.Question.Key);
            }
            */

            if (candidates.Count == 0)
            {
                Debug.Log("########## OUT OF OPTIONS############\nMissing Words:");
                foreach(KeyValuePair<string, string> q in List)
                {
                    Debug.Log(q.Key);
                }

            }

            List<WordCandidate> maxReuse = candidates.Where(c => c.ReusedLetters == candidates.Max(d => d.ReusedLetters)).ToList();
            WordCandidate chosen = maxReuse[UnityEngine.Random.Range(0, maxReuse.Count)];
            AddWordToGrid(chosen);
        }

        DrawGrid();
    }

    private void AddWordToGrid(WordCandidate wc)
    {
        int num = QCounter++;
        List.Remove(wc.Question.Key);

        string word = wc.Question.Key;

        if (wc.Horizontal)
        {
            Grid[wc.X, wc.Y] = num + "";
            for (int i = 0; i < word.Length; i++)
            {
                Grid[wc.X + i + 1, wc.Y] = word[i].ToString();
            }
            Grid[wc.X + word.Length + 1, wc.Y] = "*";
        }
        else
        {
            Grid[wc.X, wc.Y] = num + "";
            for (int i = 0; i < word.Length; i++)
            {
                Grid[wc.X, wc.Y + i + 1] = word[i].ToString();
            }
            Grid[wc.X, wc.Y + word.Length + 1] = "*";
        }

        AddText(num + " " + wc.Question.Value, 20, Color.black, FontStyle.Normal, 0, QHeight * num, 1, QHeight * (num + 1), QContainer, TextAnchor.MiddleLeft, false);
    }


    /// <summary>
    /// Add a panel element. xStart, xEnd, yStart, yEnd are percentage values (between 0 and 1).
    /// </summary>
    protected RectTransform AddPanel(string name, Color backgroundColor, float xStart, float yStart, float xEnd, float yEnd, RectTransform parent, Sprite shape = null)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        Image image = panel.AddComponent<Image>();
        image.color = backgroundColor;
        image.raycastTarget = false;
        if (shape != null) image.sprite = shape;

        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(0, 0);
        rectTransform.anchorMin = new Vector2(xStart, 1 - yEnd);
        rectTransform.anchorMax = new Vector2(xEnd, 1 - yStart);
        objects.Add(panel);

        return rectTransform;
    }

    /// <summary>
    /// Add a text element. xStart, xEnd, yStart, yEnd are percentage values (between 0 and 1).
    /// </summary>
    protected GameObject AddText(string content, int fontSize, Color fontColor, FontStyle fontStyle, float xStart, float yStart, float xEnd, float yEnd, RectTransform parent, TextAnchor textAnchor = TextAnchor.MiddleCenter, bool addToList = true)
    {
        GameObject textElement = new GameObject(content);
        textElement.transform.SetParent(parent, false);
        Text text = textElement.AddComponent<Text>();
        text.text = content;
        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        text.font = ArialFont;
        text.material = ArialFont.material;
        text.fontStyle = fontStyle;
        text.color = fontColor;
        text.fontSize = fontSize;
        text.alignment = textAnchor;
        text.raycastTarget = false;

        RectTransform textRect = textElement.GetComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(0, 0);
        textRect.sizeDelta = new Vector2(0, 0);
        textRect.anchorMin = new Vector2(xStart, 1 - yEnd);
        textRect.anchorMax = new Vector2(xEnd, 1 - yStart);
        if(addToList) objects.Add(textElement);

        return textElement;
    }

    /// <summary>
    /// Destroys all elements in this UI Element.
    /// </summary>
    protected void Clear()
    {
        foreach (GameObject go in objects) GameObject.Destroy(go);
        objects.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            AddWord();
        }
    }

    private bool IsNumeric(string s)
    {
        return (s == "" || s == null) ? false : !s.Any(c => c < '0' || c > '9');
    }
}
