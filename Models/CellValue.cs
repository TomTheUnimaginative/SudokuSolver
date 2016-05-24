using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TomsSodokuSolver.Models
{
    public class CellValue
    {
        public int CellValueRow;
        public int CellValueColumn;
        public int CellValueDigit;
        public bool CellIsFixedState; // True - Value cannot be changed.  False - Value is subject to trial and error. 
        public int CellValueState; //1 - Is a possible value for this cell. 0 - Cannot be a value for this cell. 
    }
}