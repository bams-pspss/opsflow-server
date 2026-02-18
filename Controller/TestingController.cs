using Microsoft.AspNetCore.Mvc;
using OpsFlow.Data;
using OpsFlow.Models;

namespace OpsFlow.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class TestingController(IConfiguration config) : ControllerBase
    {
        DataContextEF _entityFramework = new DataContextEF(config);


        [HttpGet()]
        public string Test()
        {
            return "This Applciation is Working!!";
        }
        [HttpGet("Testing")]
        public IEnumerable<TestEntity> Testing()
        {
            IEnumerable<TestEntity> test = _entityFramework.TestEntity.ToList<TestEntity>();
            return test;
        }

    }
}