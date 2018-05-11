using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace frogRobot.Controllers
{
    public class testController : ApiController
    {
        [HttpGet]
        public string testreturn()
        {
            return "You Get it!!";
        }
    }
}