using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Picross
{
    public class PicrossBoard
    {
        public bool hasWon { get; protected set; }

        protected static Texture2D emptySquare, squareX, filledSquare, winSquare, winSquareEmpty;
        protected static SpriteFont font;

        bool mouseOnBoard { get; set; }
        int mouseSquareX { get; set; }
        int mouseSquareY { get; set; }

        enum SquareType { EMPTY, X, FILLED };
        SquareType[,] board;

        List<int>[] rowHints, colHints;
        String[] rowHintsText;

        protected int squaresWide { get; set; }
        protected int squaresTall { get; set; }

        public const int SquareWidth = 30;
        public const int SquareHeight = 30;
        public const int Padding = 2;

        public int BoardWidth
        {
            get { return squaresWide * SquareWidth + Padding * ((squaresWide - 1) / 5); }
        }
        public int BoardHeight
        {
            get { return squaresTall * SquareHeight + Padding * ((squaresTall - 1) / 5); }
        }

        public PicrossBoard()
        {
        }

        public static void LoadContent(PicrossGame game)
        {
            emptySquare = game.Content.Load<Texture2D>(@"Images\SquareEmpty");
            filledSquare = game.Content.Load<Texture2D>(@"Images\SquareFull");
            squareX = game.Content.Load<Texture2D>(@"Images\SquareX");
            winSquare = game.Content.Load<Texture2D>(@"Images\SquareWin");
            winSquareEmpty = game.Content.Load<Texture2D>(@"Images\SquareWinEmpty");

            font = game.Content.Load<SpriteFont>(@"Fonts\NumberFont");
        }

        #region Solver Methods
        private bool autoSolve()
        {
            for (int row = 0; row < squaresTall; row++)
            {
                if (solveRow(row))
                    return true;
            }

            for (int col = 0; col < squaresWide; col++)
            {
                if (solveCol(col))
                    return true;
            }

            return false;
        }

        private bool solveCol(int col)
        {
            //consider all possible fillings-in
            //if only one of the possibilities is found, that square is forced
            bool[] canBeFilled = new bool[squaresTall];
            bool[] canBeEmpty = new bool[squaresTall];

            //nothing found yet
            for (int i = 0; i < squaresTall; i++)
            {
                canBeFilled[i] = false;
                canBeEmpty[i] = false;
            }

            //what we want to do is enumerate through each possible
            //size of the spaces between!  left edge can be 0-infty and
            //internals can be 1-infty, with the total at the leftover
            //from the hints (right edge is fixed to be the leftovers,
            //since we do have a constraint).  But we can actually assume
            //each internal gap has one extra space, and uniformly vary from 0
            int colTotal = sum(colHints[col]);
            int leftover = squaresTall - colTotal - (colHints[col].Count - 1); //the total number of variable gap positions

            foreach (LinkedList<int> gapList in generateExtraGaps(colHints[col].Count, leftover))
            {
                SquareType[] realizedCol = realizeLine(gapList, colHints[col]);

                //if the realization doesn't fit what we already know, skip this row
                bool fitsKnownSquares = true;
                for (int i = 0; i < squaresTall; i++)
                {
                    if (board[col, i] != SquareType.EMPTY && board[col, i] != realizedCol[i])
                        fitsKnownSquares = false;
                }

                if (!fitsKnownSquares)
                    continue;

                for (int i = 0; i < squaresTall; i++)
                {
                    switch (realizedCol[i])
                    {
                        case SquareType.X:
                            canBeEmpty[i] = true;
                            break;

                        case SquareType.FILLED:
                            canBeFilled[i] = true;
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            bool changeMade = false;

            for (int i = 0; i < squaresTall; i++)
            {
                if (canBeEmpty[i] && !canBeFilled[i] && board[col, i] != SquareType.X)
                {
                    board[col, i] = SquareType.X;
                    changeMade = true;
                }

                else if (canBeFilled[i] && !canBeEmpty[i] && board[col, i] != SquareType.FILLED)
                {
                    board[col, i] = SquareType.FILLED;
                    changeMade = true;
                }
            }

            return changeMade;
        }

        private bool solveRow(int row)
        {
            //consider all possible fillings-in
            //if only one of the possibilities is found, that square is forced
            bool[] canBeFilled = new bool[squaresWide];
            bool[] canBeEmpty = new bool[squaresWide];

            //nothing found yet
            for (int i = 0; i < squaresWide; i++)
            {
                canBeFilled[i] = false;
                canBeEmpty[i] = false;
            }

            //what we want to do is enumerate through each possible
            //size of the spaces between!  left edge can be 0-infty and
            //internals can be 1-infty, with the total at the leftover
            //from the hints (right edge is fixed to be the leftovers,
            //since we do have a constraint).  But we can actually assume
            //each internal gap has one extra space, and uniformly vary from 0
            int rowTotal = sum(rowHints[row]);
            int leftover = squaresWide - rowTotal - (rowHints[row].Count - 1); //the total number of variable gap positions

            foreach (LinkedList<int> gapList in generateExtraGaps(rowHints[row].Count, leftover))
            {
                SquareType[] realizedRow = realizeLine(gapList, rowHints[row]);

                //if the realization doesn't fit what we already know, skip this row
                bool fitsKnownSquares = true;
                for (int i = 0; i < squaresWide; i++)
                {
                    if (board[i, row] != SquareType.EMPTY && board[i, row] != realizedRow[i])
                        fitsKnownSquares = false;
                }

                if (!fitsKnownSquares)
                    continue;

                for (int i = 0; i < squaresWide; i++)
                {
                    switch (realizedRow[i])
                    {
                        case SquareType.X:
                            canBeEmpty[i] = true;
                            break;

                        case SquareType.FILLED:
                            canBeFilled[i] = true;
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            bool changeMade = false;

            for (int i = 0; i < squaresWide; i++)
            {
                if (canBeEmpty[i] && !canBeFilled[i] && board[i, row] != SquareType.X)
                {
                    board[i, row] = SquareType.X;
                    changeMade = true;
                }

                else if (canBeFilled[i] && !canBeEmpty[i] && board[i, row] != SquareType.FILLED)
                {
                    board[i, row] = SquareType.FILLED;
                    changeMade = true;
                }
            }

            return changeMade;
        }

        /// <summary>
        /// Returns a realized (that is, X and FILLED) line (that is, row or column)
        /// from the specified constraints.
        /// 
        /// "GapList" is the list of free gaps in the row to be realized
        /// "FilledList" is the list of filled squares in the row to be realized
        /// That is, the row XXFXXFXFXXX would have:
        ///    gapList: 2, 1, 0
        ///    filledList: 1, 1, 1
        /// 
        /// The internal gaps have an extra (so they all vary from 0, to make enumeration
        /// easier) since they have to have at least 1 (it's fixed) but the leading gap does not.
        /// The final gap is not reported, since it's always "the rest"
        /// </summary>
        /// <param name="gapList"></param>
        /// <param name="filledList"></param>
        /// <returns></returns>
        private SquareType[] realizeLine(LinkedList<int> gapList, List<int> filledList)
        {
            if (gapList.Count != filledList.Count)
                throw new ArgumentException("Gap list and hint list incompatible");

            SquareType[] row = new SquareType[squaresWide];

            int index = 0;
            int gapExtra = 0;

            int arrayPosition = 0;

            foreach(int gap in gapList)
            {
                for (int x = 0; x < gap + gapExtra; x++)
                {
                    row[index] = SquareType.X;
                    index++;
                }

                for (int x = 0; x < filledList[arrayPosition]; x++)
                {
                    row[index] = SquareType.FILLED;
                    index++;
                }

                arrayPosition += 1;
                gapExtra = 1;
            }

            while (index < squaresWide)
            {
                row[index] = SquareType.X;
                index++;
            }

            if (index > squaresWide)
                throw new ArgumentException("Too much gap space");

            return row;
        }

        private IEnumerable<LinkedList<int>> generateExtraGaps(int length, int maxSum)
        {
            if (length < 0 || maxSum < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            else if (length == 0)
            {
                yield return new LinkedList<int>();
            }
            else if (length == 1)
            {
                for (int value = 0; value <= maxSum; value++)
                {
                    LinkedList<int> output = new LinkedList<int>();
                    output.AddLast(value);
                    yield return output;
                }
            }
            else
            {
                for (int value = 0; value <= maxSum; value++)
                {
                    foreach (LinkedList<int> nextStep in generateExtraGaps(length - 1, maxSum - value))
                    {
                        LinkedList<int> output = new LinkedList<int>(nextStep);
                        output.AddFirst(value);
                        yield return output;
                    }
                }
            }
        }

        private int sum(List<int> list)
        {
            int total = 0;

            for (int i = 0; i < list.Count; i++)
                total += list[i];

            return total;
        }
        #endregion

        public void resetPuzzle()
        {
            hasWon = false;
            solverStarted = false;

            randomBoard(10, 10);
        }

        private void randomBoard(int width, int height)
        {
            squaresWide = width;
            squaresTall = height;

            //new empty board
            clearBoard();

            Random r = new Random();
            SquareType[,] solution = new SquareType[squaresWide, squaresTall];

            for (int x = 0; x < squaresWide; x++)
            {
                for (int y = 0; y < squaresTall; y++)
                {
                    if (r.NextDouble() < .6)
                        solution[x, y] = SquareType.EMPTY;
                    else
                        solution[x, y] = SquareType.FILLED;
                }
            }

            rowHints = new List<int>[squaresTall];
            rowHintsText = new String[squaresTall];

            for (int row = 0; row < squaresTall; row++)
            {
                rowHints[row] = new List<int>();
                rowHintsText[row] = "";

                int count = 0;
                bool filled = false;

                for (int x = 0; x < squaresWide; x++)
                {
                    if (filled)
                    {
                        if (solution[x, row] == SquareType.FILLED)
                        {
                            count += 1;
                        }
                        else
                        {
                            filled = false;

                            rowHints[row].Add(count);
                            rowHintsText[row] += count.ToString() + " ";
                        }
                    }
                    else
                    {
                        if (solution[x, row] == SquareType.FILLED)
                        {
                            count = 1;
                            filled = true;
                        }
                    }
                }

                if (filled)
                {
                    rowHints[row].Add(count);
                    rowHintsText[row] += count.ToString() + " ";
                }

                if (rowHints[row].Count == 0)
                {
                    rowHintsText[row] = "0";
                }
            }

            colHints = new List<int>[squaresWide];

            for (int col = 0; col < squaresWide; col++)
            {
                colHints[col] = new List<int>();

                int count = 0;
                bool filled = false;

                for (int y = 0; y < squaresTall; y++)
                {
                    if (filled)
                    {
                        if (solution[col, y] == SquareType.FILLED)
                        {
                            count += 1;
                        }
                        else
                        {
                            filled = false;

                            colHints[col].Add(count);
                        }
                    }
                    else
                    {
                        if (solution[col, y] == SquareType.FILLED)
                        {
                            count = 1;
                            filled = true;
                        }
                    }
                }

                if (filled)
                {
                    colHints[col].Add(count);
                }
            }
        }

        public void clearBoard()
        {
            hasWon = false;

            board = new SquareType[squaresWide, squaresTall];
            for (int x = 0; x < squaresWide; x++)
            {
                for (int y = 0; y < squaresTall; y++)
                {
                    board[x, y] = SquareType.EMPTY;
                }
            }
        }

        public void UpdateMousePosition(int mx, int my)
        {
            if (mx < 0 || my < 0)
            {
                mouseOnBoard = false;
                return;
            }

            mouseSquareX = mx / SquareWidth;
            mouseSquareY = my / SquareHeight;

            if (mouseSquareX >= squaresWide || mouseSquareY >= squaresTall)
            {
                mouseOnBoard = false;
            }
            else
            {
                mouseOnBoard = true;
            }
        }

        public void processClick(bool modified)
        {
            if (!mouseOnBoard)
                return;

            if (hasWon)
            {
                //ignore clicks
            }
            else
            {
                if (board[mouseSquareX, mouseSquareY] != SquareType.EMPTY)
                    board[mouseSquareX, mouseSquareY] = SquareType.EMPTY;
                else if (modified)
                    board[mouseSquareX, mouseSquareY] = SquareType.X;
                else
                    board[mouseSquareX, mouseSquareY] = SquareType.FILLED;

                detectWin();
            }
        }

        private void detectWin()
        {
            hasWon = false;

            for (int row = 0; row < squaresTall; row++)
            {
                List<int> blocksFound = new List<int>();

                bool filledIn = false;
                int currentCount = 0;

                for (int x = 0; x < squaresWide; x++)
                {
                    if (filledIn)
                    {
                        if (board[x, row] == SquareType.FILLED)
                        {
                            currentCount += 1;
                        }
                        else
                        {
                            filledIn = false;
                            blocksFound.Add(currentCount);
                        }
                    }
                    else
                    {
                        if (board[x, row] == SquareType.FILLED)
                        {
                            filledIn = true;
                            currentCount = 1;
                        }
                    }
                }

                if (filledIn)
                    blocksFound.Add(currentCount);

                if (!SameList(blocksFound, rowHints[row]))
                    return;
            }

            for (int col = 0; col < squaresWide; col++)
            {
                List<int> blocksFound = new List<int>();

                bool filledIn = false;
                int currentCount = 0;

                for (int y = 0; y < squaresTall; y++)
                {
                    if (filledIn)
                    {
                        if (board[col, y] == SquareType.FILLED)
                        {
                            currentCount += 1;
                        }
                        else
                        {
                            filledIn = false;
                            blocksFound.Add(currentCount);
                        }
                    }
                    else
                    {
                        if (board[col, y] == SquareType.FILLED)
                        {
                            filledIn = true;
                            currentCount = 1;
                        }
                    }
                }

                if (filledIn)
                    blocksFound.Add(currentCount);

                if (!SameList(blocksFound, colHints[col]))
                    return;
            }

            hasWon = true;
        }

        private static bool SameList<T>(List<T> A, List<T> B)
        {
            if (A.Count != B.Count)
                return false;

            for (int i = 0; i < A.Count; i++)
            {
                if (!A[i].Equals(B[i]))
                    return false;
            }

            return true;
        }

        #region Drawing methods
        public void Draw(SpriteBatch spriteBatch, int xOffset, int yOffset)
        {
            drawBoard(spriteBatch, xOffset, yOffset);

            if (!hasWon)
            {
                drawRowHints(spriteBatch, xOffset, yOffset);
                drawColumnHints(spriteBatch, xOffset, yOffset);
            }
        }

        private void drawColumnHints(SpriteBatch spriteBatch, int xOffset, int yOffset)
        {
            int rowHeight = 20;

            int xTranslation = xOffset + 11;
            int yTranslation = yOffset + 2 + BoardHeight;

            for (int col = 0; col < squaresWide; col++)
            {
                yTranslation = yOffset + 2 + BoardHeight;

                for (int i = 0; i < colHints[col].Count; i++)
                {
                    spriteBatch.DrawString(
                        font,
                        colHints[col][i].ToString(),
                        new Vector2(xTranslation, yTranslation),
                        Color.White
                        );

                    yTranslation += rowHeight;
                }

                if (colHints[col].Count == 0)
                {
                    spriteBatch.DrawString(
                        font,
                        "0",
                        new Vector2(xTranslation, yTranslation),
                        Color.White
                        );
                }

                xTranslation += SquareWidth;
                if (col % 5 == 4)
                    xTranslation += Padding;
            }
        }

        private void drawRowHints(SpriteBatch spriteBatch, int xOffset, int yOffset)
        {
            int xTranslation = xOffset + 5 + BoardWidth;
            int yTranslation = yOffset + 2;

            for (int row = 0; row < squaresTall; row++)
            {
                spriteBatch.DrawString(
                    font,
                    rowHintsText[row],
                    new Vector2(xTranslation, yTranslation),
                    Color.White
                    );

                yTranslation += SquareHeight;
                if (row % 5 == 4)
                    yTranslation += Padding;
            }
        }

        private void drawBoard(SpriteBatch spriteBatch, int xOffset, int yOffset)
        {
            int xTranslation = xOffset;
            int yTranslation = yOffset;

            for (int x = 0; x < squaresWide; x++)
            {
                yTranslation = yOffset;

                for (int y = 0; y < squaresTall; y++)
                {
                    spriteBatch.Draw(pickSquareTexture(x, y),
                        new Vector2(xTranslation, yTranslation),
                        Color.White);

                    yTranslation += SquareHeight;
                    if (y % 5 == 4)
                        yTranslation += Padding;
                }

                xTranslation += SquareWidth;
                if (x % 5 == 4)
                    xTranslation += Padding;
            }
        }

        Texture2D pickSquareTexture(int x, int y)
        {
            if (hasWon)
            {
                switch (board[x, y])
                {
                    case SquareType.FILLED:
                        return winSquare;

                    default:
                        return winSquareEmpty;
                }
            }
            else
            {
                switch (board[x, y])
                {
                    case SquareType.EMPTY:
                        return emptySquare;

                    case SquareType.FILLED:
                        return filledSquare;

                    case SquareType.X:
                        return squareX;

                    default:
                        throw new NotImplementedException();
                }
            }
        }
        #endregion

        private int ticksBetweenAutoSolve = 15;
        private int ticksSinceAutoSolve = 0;

        private bool solverStarted { get; set; }

        public void StartAutoSolver()
        {
            solverStarted = true;
            ticksSinceAutoSolve = ticksBetweenAutoSolve;
        }

        public void Update()
        {
            if (solverStarted)
            {
                if (ticksSinceAutoSolve < ticksBetweenAutoSolve)
                {
                    ticksSinceAutoSolve++;
                }
                else
                {
                    solverStarted = autoSolve();
                    detectWin();
                }
            }
        }
    }
}
