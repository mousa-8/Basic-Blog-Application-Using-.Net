using PIP.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PIP.Controllers
{
    public class DataImportController : Controller
    {
        private readonly Database1Entities1 _dbContext;

        public DataImportController()
        {
            _dbContext = new Database1Entities1();
        }

        // GET: Student  
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ImportFile()
        {
            return View("Index");
        }
        private List<Datum> GetDataFromCSVFile(Stream stream)
        {
            var empList = new List<Datum>();
            try
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                {
                    var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true // To set First Row As Column Names    
                        }
                    });

                    if (dataSet.Tables.Count > 0)
                    {
                        var dataTable = dataSet.Tables[0];
                        foreach (DataRow objDataRow in dataTable.Rows)
                        {
                            if (objDataRow.ItemArray.All(x => string.IsNullOrEmpty(x?.ToString()))) continue;
                            empList.Add(new Datum()
                            {
                                Id = Convert.ToInt32(objDataRow["ID"].ToString()),
                                data1 = objDataRow["Name"].ToString(),
                                data2 = objDataRow["Position"].ToString(),
                                data3 = objDataRow["Location"].ToString(),                               
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return empList;
        }
    }
}