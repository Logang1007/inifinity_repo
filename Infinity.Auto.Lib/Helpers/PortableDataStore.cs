using Infinity.Automation.Lib.Models;
using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Infinity.Automation.Lib.Models.Enums;

namespace Infinity.Automation.Lib.Helpers
{
    public interface IPortableDataStore
    {
        string DBFullPath { get; set; }
        bool EnablePortablDB { get; set; }
        void AddTestRunResults(TestResponseDTO testResponseDTO);
        List<TestHeaderEntity> GetTestHeadersForToday();
        List<TestHeaderEntity> GetTestHeadersAll();
        ChartDTO GetChartDataForToday();
    }

    public class TestHeaderEntity
    {
        public string TestUniqueCode { get; set; }
        public string TestId { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string TestIncludeToRunFirst { get; set; }
        public string SimulateNetworkCondition { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime TestStartTime { get; set; }
        public DateTime TestEndTime { get; set; }
        public bool IsTestPassed { get; set; }
        public string ResponseMessage { get; set; }
        public string OutputResultsFullFilePath { get; set; }
        public int NumberOfIterations { get; set; }
    }




    public class PortableDataStore: IPortableDataStore
    {
        public string DBFullPath { get; set; }
        public bool EnablePortablDB { get; set; }

        public void AddTestRunResults(TestResponseDTO testResponseDTO)
        {
            if (EnablePortablDB)
            {
                using (var db = new LiteDatabase(this.DBFullPath))
                {
                    var dateNow = DateTime.Now;
                    var headerEntity = db.GetCollection<TestHeaderEntity>("TestHeader");

                    // Create your new customer instance
                    var testHeaderEntity = new TestHeaderEntity
                    {
                        TestUniqueCode = testResponseDTO.TestDetailDTO.TestUniqueCode,
                        AddedDate = dateNow,
                        TestId = testResponseDTO.TestDetailDTO.TestId,
                        Name = testResponseDTO.TestDetailDTO.Name,
                        Author = testResponseDTO.TestDetailDTO.Author,
                        Description = testResponseDTO.TestDetailDTO.Description,
                        TestIncludeToRunFirst = testResponseDTO.TestDetailDTO.TestIncludeToRunFirst,
                        SimulateNetworkCondition = JsonConvert.SerializeObject(testResponseDTO.TestDetailDTO.SimulateNetworkCondition),
                        TestStartTime = testResponseDTO.TestDetailDTO.TimeTaken.StartTime,
                        TestEndTime = testResponseDTO.TestDetailDTO.TimeTaken.EndTime,
                        IsTestPassed = !testResponseDTO.IsTestFailed,
                        ResponseMessage = testResponseDTO.ResponseMessage,
                        OutputResultsFullFilePath = testResponseDTO.TestDetailDTO.OutputFullFilePath,
                        NumberOfIterations= testResponseDTO.TestDetailDTO.NumberOfIterations,

                    };
                    headerEntity.Insert(testHeaderEntity);

                    //insert commands
                    var commandEntity = db.GetCollection<CommandDTO>("TestCommands");
                    
                    foreach (var item in testResponseDTO.CommandsExecuted)
                    {
                        bool isFailed = item.CommandStatus != CommandResponseStatus.Success;
                        var commandEntityRec = item;
                        commandEntityRec.TestId = testResponseDTO.TestDetailDTO.TestId;
                        commandEntityRec.TestUniqueCode = testResponseDTO.TestDetailDTO.TestUniqueCode;
                        commandEntityRec.AddedDate = dateNow;
                        commandEntityRec.DurationStartTime = commandEntityRec.TimeTaken.StartTime;
                        commandEntityRec.DurationEndTime = commandEntityRec.TimeTaken.EndTime;
                        commandEntityRec.IsFailed = isFailed;
                        commandEntityRec.ResponseMessage = item.Message;
                        commandEntity.Insert(commandEntityRec);
                    }


                }
            }
            
        }

        public List<TestHeaderEntity> GetTestHeadersAll()
        {
            if (EnablePortablDB)
            {
                using (var db = new LiteDatabase(this.DBFullPath))
                {
                    var headerEntity = db.GetCollection<TestHeaderEntity>("TestHeader");
                    return headerEntity.FindAll().ToList();
                }

            }
            return new List<TestHeaderEntity>();
        }

        public List<TestHeaderEntity> GetTestHeadersForToday()
        {
            if (EnablePortablDB)
            {
                var startOfDay = DateTime.Now.StartOfDay();
                using (var db = new LiteDatabase(this.DBFullPath))
                {
                    var headerEntity = db.GetCollection<TestHeaderEntity>("TestHeader");
                    return  headerEntity.Find(x => x.AddedDate >= startOfDay).ToList();
                }

            }
            return new List<TestHeaderEntity>();
        }

        public ChartDTO GetChartDataForToday()
        {
            ChartDTO chartDTO = new ChartDTO();
            var testHeaderData = this.GetTestHeadersForToday().GroupBy(x => x.Name).Select(x => x.Key).ToList();
            if (testHeaderData.Count == 0)
            {
                return chartDTO;
            }
            
            chartDTO.labels = new List<string>();
            foreach (var item in testHeaderData)
            {
                chartDTO.labels.Add(item);
            }
            chartDTO.datasets = new List<ChartDataset>();
            chartDTO.datasets.Add(new ChartDataset() { label = "Failed", backgroundColor = "#FF6384", data = null, maxBarThickness = 4 });
            chartDTO.datasets.Add(new ChartDataset() { label = "Success", backgroundColor = "#4BC0C0", data = null, maxBarThickness = 4 });

            var testCountFailed = this.GetTestHeadersAll().Where(x => !x.IsTestPassed).GroupBy(x => x.Name).Select(x => new { TestName = x.Key, Count = x.Count() }).ToList();
            chartDTO.datasets[0].data = new double[testCountFailed.Count()];
            int count = 0;
            foreach (var item in testCountFailed)
            {
                chartDTO.datasets[0].data[count] = item.Count;
                count++;
            }
            var testCountPassed = this.GetTestHeadersAll().Where(x => x.IsTestPassed).GroupBy(x => x.Name).Select(x => new { TestName = x.Key, Count = x.Count() }).ToList();
            chartDTO.datasets[1].data = new double[testCountPassed.Count()];
            count = 0;
            foreach (var item in testCountPassed)
            {
                chartDTO.datasets[1].data[count] = item.Count;
                count++;
            }

            return chartDTO;
        }
    }
}
