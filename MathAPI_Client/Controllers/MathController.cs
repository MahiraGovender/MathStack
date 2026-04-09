using System.Text;
using Firebase.Auth;
using MathAPI_Client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace MathAPI_Client.Controllers
{
    public class MathController : Controller
    {
        private static HttpClient httpClient = new()
        {
            BaseAddress = new Uri("http://localhost:5151"),
        };

        [HttpGet]
        public IActionResult Calculate()
        {
           var token = HttpContext.Session.GetString("MathJWT");

            if (token == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            List<SelectListItem> operations = new List<SelectListItem> {
            new SelectListItem { Value = "1", Text = "+" },
            new SelectListItem { Value = "2", Text = "-" },
            new SelectListItem { Value = "3", Text = "*" },
            new SelectListItem { Value = "4", Text = "/" },

            };

            ViewBag.Operations = operations;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(decimal? FirstNumber, decimal? SecondNumber,int Operation)
        {
            var token = HttpContext.Session.GetString("MathJWT");

            if (token == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var currentUser = HttpContext.Session.GetString("currentUser");
            decimal? Result = 0;
            MathCalculation mathCalculation;

            try
            {
                mathCalculation = MathCalculation.Create(FirstNumber, SecondNumber, Operation, Result, currentUser);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
                throw;
            }
            
            StringContent jsonContent = new(JsonConvert.SerializeObject(mathCalculation), Encoding.UTF8,"application/json"); 
            
            HttpResponseMessage response = await httpClient.PostAsync("api/Math/PostCalculate", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                MathCalculation? deserialisedResponse = JsonConvert.DeserializeObject<MathCalculation>(jsonResponse);
                ViewBag.Result = deserialisedResponse.Result;
                List<SelectListItem> operations = new List<SelectListItem> 
                {
                    new SelectListItem { Value = "1", Text = "+" },
                    new SelectListItem { Value = "2", Text = "-" },
                    new SelectListItem { Value = "3", Text = "*" },
                    new SelectListItem { Value = "4", Text = "/" },
                };
                ViewBag.Operations = operations;

                return View();
            }
            else
            {
                ViewBag.Result = "An error has occurred";
                return View();
            }
        }

        //MathAPI MathController: PostCalculate

        /*[HttpPost("PostCalculate")]
        [ProducesResponseType(typeof(MathCalculation), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Error), StatusCodes.Status401Unauthorized)]
        [Produces("application/json")]
        public async Task<IActionResult> PostCalculate(MathCalculation mathCalculation)
        {
            var Result = mathCalculation.Result;

            if (mathCalculation.FirebaseUuid == null || mathCalculation.FirebaseUuid == "")
            {
                return Unauthorized(new Error("Token missing!"));
            }

            if (mathCalculation.FirstNumber == null || mathCalculation.SecondNumber == null || mathCalculation.Operation == 0)
            {
                return BadRequest(new Error("Math equation not complete!"));
            }

            try
            {
                mathCalculation = MathCalculation.Create(mathCalculation.FirstNumber, mathCalculation.SecondNumber, mathCalculation.Operation, Result, mathCalculation.FirebaseUuid);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


            switch (mathCalculation.Operation)
            {
                case 1:
                    mathCalculation.Result = mathCalculation.FirstNumber + mathCalculation.SecondNumber;
                    break;
                case 2:
                    mathCalculation.Result = mathCalculation.FirstNumber - mathCalculation.SecondNumber;
                    break;
                case 3:
                    mathCalculation.Result = mathCalculation.FirstNumber * mathCalculation.SecondNumber;
                    break;
                default:
                    mathCalculation.Result = mathCalculation.FirstNumber / mathCalculation.SecondNumber;
                    break;
            }

            if (ModelState.IsValid)
            {
                _context.Add(mathCalculation);
                await _context.SaveChangesAsync();

            }

            return Created(mathCalculation.CalculationId.ToString(), mathCalculation);

        }*/

        //MathApp MathController: Calculate
        /*  [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Calculate(decimal? FirstNumber, decimal? SecondNumber,int Operation)
            {
                var token = HttpContext.Session.GetString("currentUser");

                if (token == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                decimal? Result = 0;
                MathCalculation mathCalculation;

                try
                {
                    mathCalculation = MathCalculation.Create(FirstNumber, SecondNumber, Operation, Result, token);
                }
                catch (Exception ex)
                {
                    ViewBag.Error = ex.Message;
                    return View();
                    throw;
                }
                

                switch (Operation)
                {
                    case 1:
                        mathCalculation.Result = FirstNumber + SecondNumber;
                        break;
                    case 2:
                        mathCalculation.Result = FirstNumber - SecondNumber;
                        break;
                    case 3:
                        mathCalculation.Result = FirstNumber * SecondNumber;
                        break;
                    default:
                        mathCalculation.Result = FirstNumber / SecondNumber;
                        break;
                }

                if (ModelState.IsValid)
                {
                    _context.Add(mathCalculation);
                    await _context.SaveChangesAsync();
                    
                }
                ViewBag.Result = mathCalculation.Result;
                return View();
                
            }*/

        public async Task<IActionResult> History()
        {
            var token = HttpContext.Session.GetString("MathJWT");

            if (token == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            
             httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await httpClient.GetAsync("/api/Math/GetHistory");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                List<MathCalculation>? deserialisedResponse = JsonConvert.DeserializeObject<List<MathCalculation>>(jsonResponse);
                if (deserialisedResponse.Count == 0)
                {
                    ViewBag.HistoryMessage = "No history exists";
                }
                return View(deserialisedResponse);
            }  else
            {
                ViewBag.HistoryMessage = "No history to show";
                return View();
            }            
        }

        //MathAPI MathController: GetHistory
        /*
            [HttpGet("GetHistory")]
            [ProducesResponseType(typeof(List<MathCalculation>), StatusCodes.Status200OK)]
            [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
            [ProducesResponseType(typeof(Error), StatusCodes.Status401Unauthorized)]
            [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
            [Produces("application/json")]
            public async Task<IActionResult> GetHistory(string Token)
            {

                if (Token == null)
                {
                    return Unauthorized(new Error("Token missing!"));
                }

                if (_context.MathCalculations.Count(m => m.FirebaseUuid.Equals(Token)) == 0)
                {
                    return Unauthorized(new Error("No history found!"));
                }

                List<MathCalculation> historyItems = await _context.MathCalculations.Where(m => m.FirebaseUuid.Equals(Token)).ToListAsync();

                return Ok(historyItems);
            }
        */

        //MathApp MathController: History
        /*
            public async Task<IActionResult> History()
            {
                var token = HttpContext.Session.GetString("currentUser");

                if (token == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                return View(await _context.MathCalculations.Where(m => m.FirebaseUuid.Equals(token)).ToListAsync());
            }
        */

 
        public async Task<IActionResult> Clear()
        {
            var token = HttpContext.Session.GetString("MathJWT");

            if (token == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await httpClient.DeleteAsync("/api/Math/DeleteHistory");;

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
            }
            return RedirectToAction("History");
        }

        //MathAPI MathController: DeleteHistory
        /*
            [HttpDelete("DeleteHistory")]
            [ProducesResponseType(typeof(List<MathCalculation>), StatusCodes.Status200OK)]
            [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
            [ProducesResponseType(typeof(Error), StatusCodes.Status401Unauthorized)]
            [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
            [Produces("application/json")]
            public async Task<IActionResult> DeleteHistory(string Token)
            {            
                if (Token == null)
                {
                    return Unauthorized(new Error("Token missing!"));
                }

                if (_context.MathCalculations.Count(m => m.FirebaseUuid.Equals(Token)) == 0)
                {
                    return Unauthorized(new Error("Token invalid!"));
                }

                List<MathCalculation> removableItems = await _context.MathCalculations.Where(m => m.FirebaseUuid.Equals(Token)).ToListAsync();

                if (removableItems.Count > 0)
                {
                    _context.MathCalculations.RemoveRange(removableItems);
                    await _context.SaveChangesAsync();
                    return Ok(removableItems);
                }
                else
                {
                    return NotFound(new Error("No history to delete!"));
                }
            }

        */

        //MathApp MathController: Clear
        /*
            public IActionResult Clear()
            {
                var token = HttpContext.Session.GetString("currentUser");

                if (token == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                _context.MathCalculations.RemoveRange(_context.MathCalculations.Where(m => m.FirebaseUuid.Equals(token)));
                _context.SaveChangesAsync();

                return RedirectToAction("History");
            }
        */
    }

}