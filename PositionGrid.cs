﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PositionGrid : MonoBehaviour {
    // This is the text to display who's turn it is or who won.
    public GameObject turnText;

    // grid should have 4 elements in order VerticalLeft, VerticalRight, HorizontalUp, HorizontalDown
    public RectTransform[] grid;

    // This should be all the grid buttons for placing an X or O
    public GameObject[] boxes;

    bool playerTurn = true;
    bool active = true;

    void btnFunc(GameObject box) {
        // If the game is done (inactive), don't do anything (this only happens when somebody won)
        if (!active)
            return;

        PositionState state = box.GetComponent<PositionState>();

        // Don't change the position if it's already set
        if (state.state != -1)
            return;

        state.state = playerTurn ? 1 : 0;
        box.GetComponent<Button>().GetComponentInChildren<Text>().text = playerTurn ? "X" : "O";

        playerTurn = !playerTurn;

        int winner = checkForWinner();

        Text turns = turnText.GetComponent<Text>();
        if (winner == -1) {
            turns.text = "It's " + (playerTurn ? "X" : "O") + "'s turn!";
        } else {
            active = false;
            if (winner == -2)
                turns.text = "It's a draw!";
            else
                turns.text = "Player " + (winner == 3 ? "X" : "O") + " won!";
            StartCoroutine(startReset(3));
        }
    }

    void updateGrid() {
        // Turn Text
        turnText.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 2 - Screen.height / 2, Screen.height);

        // Grid lines
        int posX = Screen.width / 2 - Screen.height / 2;
        Vector2 ver = new Vector2(32, Screen.height);
        Vector2 hor = new Vector2(Screen.height, 32);

        for (int x = 0; x < 2; x++) {
            grid[x].anchoredPosition = new Vector2(posX + (Screen.height / 3) * (x + 1), 0);
            grid[x].sizeDelta = ver;
        }

        for (int y = 2; y < 4; y++) {
            grid[y].anchoredPosition = new Vector2(posX, -((Screen.height / 3) * (y - 1)));
            grid[y].sizeDelta = hor;
        }

        // Buttons
        Vector2 btnSize = new Vector2(Screen.height / 3, Screen.height / 3);

        int i = 0;
        for (int x = 0; x < 3; x++) {
            for (int y = 0; y < 3; y++) {
                if (i > boxes.Length)
                    break;

                GameObject box = boxes[i++];
                RectTransform tran = box.GetComponent<RectTransform>();
                tran.anchoredPosition = new Vector2(posX + (Screen.height / 3 * x), -(Screen.height / 3 * y));
                tran.sizeDelta = btnSize;

                box.GetComponent<Button>().onClick.AddListener(delegate { btnFunc(box); });
                // box.GetComponent<Button>().GetComponentInChildren<Text>().text = i.ToString();
            }
        }
    }

    // Returns -2 (draw),  -1 (no winner), 0 (O won), or 3 (X won)
    int checkForWinner() {
        // Grid layout:
        // 0  3  6
        // 1  4  7
        // 2  5  8

        // Check horizontal positions
        for (int i = 0; i < 3; i++) {
            int sum = 0;
            for (int x = 0; x < 3; x++) {
                //            (012) +  (036) = (036)(147)(258)
                int state = boxes[i + (x * 3)].GetComponent<PositionState>().state;

                // Break early if this block isn't filled in
                if (state == -1) {
                    sum = -1;
                    break;
                } else {
                    sum += state;
                }
            }

            // The sum should be 0 or 3 if it's a winner
            if (sum == 0 || sum == 3)
                return sum;
        }

        // Check vertical positions
        for (int i = 0; i < 3; i++) {
            int sum = 0;
            for (int x = 0; x < 3; x++) {
                //                 (036)  + (012) = (012)(345)(678)
                int state = boxes[(i * 3) + x].GetComponent<PositionState>().state;

                // Break early if this block isn't filled in
                if (state == -1) {
                    sum = -1;
                    break;
                } else {
                    sum += state;
                }
            }

            // The sum should be 0 or 3 if it's a winner
            if (sum == 0 || sum == 3)
                return sum;
        }

        // Check diagonal positions------------------------------------
        // Top left to bottom right
        int sumTL = 0;
        for (int x = 0; x < 3; x++) {
            //                (048)
            int state = boxes[x * 4].GetComponent<PositionState>().state;

            // Break early if this block isn't filled in
            if (state == -1) {
                sumTL = -1;
                break;
            } else {
                sumTL += state;
            }
        }
        if (sumTL == 0 || sumTL == 3)
            return sumTL;

        // Bottom left to top right
        int sumBL = 0;
        for (int x = 0; x < 3; x++) {
            //                2 + (024) = (246)
            int state = boxes[2 + x * 2].GetComponent<PositionState>().state;

            // Break early if this block isn't filled in
            if (state == -1) {
                sumBL = -1;
                break;
            } else {
                sumBL += state;
            }
        }
        if (sumBL == 0 || sumBL == 3)
            return sumBL;

        // Now double check to make sure there is an empty space. If all are full and we got this far, then there's a draw
        int filled = 0;
        foreach (GameObject box in boxes) {
            if (box.GetComponent<PositionState>().state != -1) {
                filled++;
            } else { // If one cell isn't filled, then we know that it's not a draw
                break;
            }
        }
        if (filled == 9)
            return -2;

        return -1;
    }

    void reset() {
        foreach (GameObject box in boxes) {
            box.GetComponent<PositionState>().state = -1;
            box.GetComponent<Button>().GetComponentInChildren<Text>().text = "";
        }
        turnText.GetComponent<Text>().text = "It's X's turn!";
        playerTurn = true;
        active = true;
    }

    IEnumerator startReset(int time) {
        yield return new WaitForSeconds(time);
        reset();
    }

    void Start() {
        updateGrid();
    }

    void Update() {
        if (Input.GetKey(KeyCode.Escape)) {
            Application.Quit(); 
        }
    }
}
