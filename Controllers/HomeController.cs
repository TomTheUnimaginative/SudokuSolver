using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TomsSodokuSolver.Models;

namespace TomsSodokuSolver.Controllers
{
    public class HomeController : Controller
    {
        public List<Models.Cell> Cells = new List<Models.Cell>();
        public List<Models.CellValue> CellValues = new List<Models.CellValue>();
        public string cumulativeMessages = "";

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            ProcessFile(file);
            return View();
        }
        public ActionResult About()
        {
            ViewBag.Message = "This program takes in text files containing Sudoku puzzles, and solves them, if possible.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact.";

            return View();
        }

        private void ProcessFile(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                InitializeObjects();
                if (LoadPuzzleFromFile(file))
                {
                    // TODO IdentifyForcedCellValues();  /// This reduces length of trial-and-error
                    if (SolvedForRemainingOpenCells(Cells, CellValues))
                    {
                        OutputSolutionFile(file);
                        ViewBag.Message = Path.GetFileName(file.FileName) + " has been succesfully processed.";
                    }
                    else
                    {
                        ViewBag.Message = Path.GetFileName(file.FileName) + " is not solveable.";
                    }
                }
                else
                {
                    ViewBag.Message = cumulativeMessages;
                }
            }
            else
            {
                ViewBag.Message = "Please select an input file before clicking ''Solve''.";
            }
        }

        private void OutputSolutionFile(HttpPostedFileBase file)
        {
            string[] outputLines = new string[9];
            var outputFileName = Server.MapPath(Path.GetDirectoryName(file.FileName)) +
                "\\" + Path.GetFileName(file.FileName.ToString()) + ".sln.txt";

            for (int row = 0; row < 9; row++)
            {
                for (int column = 0; column < 9; column++)
                {
                    var selectedCellValue = Cells.FirstOrDefault(
                                                                        x => x.CellRow == row &&
                                                                        x.CellColumn == column);
                    if (selectedCellValue == null)
                    {
                        // Added for testing/debugging.
                        // Shouldn't happen in real life.
                        outputLines[row] = outputLines[row] + "?";
                    }
                    else
                    {
                        outputLines[row] = outputLines[row] + selectedCellValue.CellContent.ToString();
                    }
                }
            }
            System.IO.File.WriteAllLines(outputFileName, outputLines);
            return;
        }

        private bool SolvedForRemainingOpenCells(List<Models.Cell> Cells, List<Models.CellValue> CellValues)
        {
            bool puzzleIsSolvable = false;
            int selectedRow = 0;
            int selectedColumn = 0;

            if (!EmptyCellFoundIn(Cells, ref selectedRow, ref selectedColumn))
            {
                puzzleIsSolvable = true;
            }
            else
            {
                foreach (Models.CellValue selectedCellValue
                    in CellValues.Where(x => x.CellValueColumn == selectedColumn &&
                                           x.CellValueRow == selectedRow &&
                                           x.CellValueState == 1))
                {
                    int selectedValue = selectedCellValue.CellValueDigit;
                    if (ValueIsValidFor(Cells, selectedRow, selectedColumn, selectedValue))
                    {
                        var selectedCell = Cells.FirstOrDefault(x => x.CellRow == selectedRow &&
                                                                       x.CellColumn == selectedColumn);
                        selectedCell.CellContent = selectedValue;
                        List<Cell> NewSetOfCells = new List<Cell>();
                        UpdateNewSetOfCells(Cells, NewSetOfCells);
                        if (SolvedForRemainingOpenCells(NewSetOfCells, CellValues))
                        {
                            puzzleIsSolvable = true;
                            UpdateOldSetOfCells(NewSetOfCells, Cells);
                            return puzzleIsSolvable;
                        }
                        else
                        {
                            selectedCell.CellContent = 0;
                        }
                    }
                }
            }
            return puzzleIsSolvable;
        }

        private void UpdateOldSetOfCells(List<Cell> newSetOfCells, List<Cell> cells)
        {
            cells.Clear();
            foreach (Cell newCell in newSetOfCells)
            {
                Models.Cell oldCell = new Models.Cell();
                oldCell.CellColumn = newCell.CellColumn;
                oldCell.CellContent = newCell.CellContent;
                oldCell.CellRow = newCell.CellRow;
                oldCell.CellSquare = newCell.CellSquare;
                cells.Add(newCell);
            }
        }

        private void UpdateNewSetOfCells(List<Cell> cells, List<Cell> newSetOfCells)
        {
            foreach (Cell oldCell in cells)
            {
                Models.Cell newCell = new Models.Cell();
                newCell.CellColumn = oldCell.CellColumn;
                newCell.CellContent = oldCell.CellContent;
                newCell.CellRow = oldCell.CellRow;
                newCell.CellSquare = oldCell.CellSquare;
                newSetOfCells.Add(newCell);
            }
        }

        private bool ValueIsValidFor(List<Cell> cells, int selectedRow, int selectedColumn, int selectedValue)
        {
            //if (selectedRow == 4)
            //{
            //int tomtest = 1;
            //}
            bool isGoodValue = true;
            int selectedSquare = Convert.ToInt32((selectedRow / 3)) * 3 + Convert.ToInt32((selectedColumn / 3));

            var selectedCell = cells.FirstOrDefault(x => x.CellRow == selectedRow &&
                                                         x.CellContent == selectedValue);
            if (!(selectedCell == null))
            {
                isGoodValue = false;
            }

            var selectedCell2 = cells.FirstOrDefault(x => x.CellColumn == selectedColumn &&
                                                                       x.CellContent == selectedValue);
            if (!(selectedCell2 == null))
            {
                isGoodValue = false;
            }

            var selectedCell3 = cells.FirstOrDefault(x => x.CellSquare == selectedSquare &&
                                                                      x.CellContent == selectedValue);
            if (!(selectedCell3 == null))
            {
                isGoodValue = false;
            }

            return isGoodValue;
        }

        private bool EmptyCellFoundIn(List<Cell> Cells, ref int selectedRow, ref int selectedColumn)
        {
            bool emptyCellFound = false;
            var selectedCell = Cells.FirstOrDefault(x => x.CellContent == 0);
            if (selectedCell == null)
            {
                emptyCellFound = false;
            }
            else
            {
                emptyCellFound = true;
                selectedRow = selectedCell.CellRow;
                selectedColumn = selectedCell.CellColumn;
            }
            return emptyCellFound;
        }

        private void IdentifyForcedCellValues()
        {
            // TODO:  If implemented, this uses deduction to reduce the number of trial-and-error
            //        runs.
        }

        private bool LoadPuzzleFromFile(HttpPostedFileBase file)
        {
            bool loadSucceeded = true;
            var fileName = Path.GetFileName(file.FileName);
            using (StreamReader sr = new StreamReader(file.InputStream))
            {
                string gridRow;
                int currentRow = 0;
                while ((gridRow = sr.ReadLine()) != null)
                {
                    if (gridRow.Length != 9)
                    {
                        loadSucceeded = false;
                        cumulativeMessages = cumulativeMessages +
                                              "Invalid row length in " + fileName + " at row " +
                                              (currentRow + 1) + ".  Each row should have exactly 9 characters.  ";
                    }
                    else
                    {
                        if (loadSucceeded)
                        {
                            int currentColumn = 0;

                            foreach (char initalCellValue in gridRow)
                            {
                                if (IsNumeric(initalCellValue.ToString(),
                                    System.Globalization.NumberStyles.Number)) // Allow any non-numeric character, not just "X" to
                                                                               // designate an uncommitted cell.
                                {
                                    int intialDigit = Convert.ToInt32(initalCellValue.ToString());
                                    int currentSquare = Convert.ToInt32((currentRow / 3)) * 3 + Convert.ToInt32((currentColumn / 3));

                                    var selectedCell = Cells.FirstOrDefault(x => x.CellRow == currentRow &&
                                                                        x.CellColumn == currentColumn);

                                    selectedCell.CellContent = intialDigit;

                                    foreach (Models.CellValue selectedCellValue in CellValues)
                                    {
                                        if (selectedCellValue.CellValueRow == currentRow &&
                                           selectedCellValue.CellValueColumn == currentColumn)
                                        {
                                            if (selectedCellValue.CellValueDigit == intialDigit)
                                            {
                                                selectedCellValue.CellIsFixedState = true;
                                                selectedCellValue.CellValueState = 1;
                                            }
                                            else
                                            {
                                                selectedCellValue.CellIsFixedState = true;
                                                selectedCellValue.CellValueState = 0;
                                            }
                                        }
                                        if (selectedCellValue.CellValueRow == currentRow &&
                                            selectedCellValue.CellValueDigit == intialDigit &&
                                            !(selectedCellValue.CellValueColumn == currentColumn))
                                        {
                                            selectedCellValue.CellIsFixedState = true;
                                            selectedCellValue.CellValueState = 0;
                                        }
                                        if (selectedCellValue.CellValueColumn == currentColumn &&
                                           selectedCellValue.CellValueDigit == intialDigit &&
                                           !(selectedCellValue.CellValueRow == currentRow))
                                        {
                                            selectedCellValue.CellIsFixedState = true;
                                            selectedCellValue.CellValueState = 0;
                                        }
                                    }
                                }
                                currentColumn++;
                            }
                        }
                    }
                    currentRow++;
                }
            }
            return loadSucceeded;
        }

        private void InitializeObjects()
        {
            for (int currentRow = 0; currentRow < 9; currentRow++)
            {
                for (int currentColumn = 0; currentColumn < 9; currentColumn++)
                {
                    Models.Cell newCell = new Models.Cell();
                    newCell.CellRow = currentRow;
                    newCell.CellColumn = currentColumn;
                    newCell.CellSquare = Convert.ToInt32((currentRow / 3)) * 3 + Convert.ToInt32((currentColumn / 3));
                    newCell.CellContent = 0;
                    Cells.Add(newCell);

                    for (int currentDigit = 1; currentDigit < 10; currentDigit++)
                    {
                        Models.CellValue newCellValue = new Models.CellValue();
                        newCellValue.CellValueRow = currentRow;
                        newCellValue.CellValueColumn = currentColumn;
                        newCellValue.CellValueDigit = currentDigit;
                        newCellValue.CellIsFixedState = false;
                        newCellValue.CellValueState = 1;
                        CellValues.Add(newCellValue);
                    }
                }
            }
        }

        public bool IsNumeric(string val, System.Globalization.NumberStyles numberStyle)
        {
            double result;
            return double.TryParse(
                val,
                numberStyle,
                System.Globalization.CultureInfo.CurrentCulture,
                out result);
        }

    }
}