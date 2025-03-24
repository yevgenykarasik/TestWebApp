using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TestWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        public TestController(IConfiguration configuration)
        {


        }

        [HttpGet("HealthCheck")]
        public IActionResult HealthCheck()
        {
            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpPost("SendMsg")]
        [Consumes("application/json")]
        public IActionResult SendMsg([FromBody] AccountPhoneAndMsgJSON accounPhoneAndMsgJson)
        {
            var accountId = accounPhoneAndMsgJson.AccountID;
            var phoneNumber = accounPhoneAndMsgJson.PhoneNumber;
            var msg = accounPhoneAndMsgJson.Message;

            try
            {
                if (!TestMicroservice.numberOfMsgsSentFromAccount.TryGetValue(accountId, out int numberOfMsgsSentFromTheAccount))
                {
                    return StatusCode(StatusCodes.Status417ExpectationFailed, "could not get the number of msgs sent from account " + accountId);
                }
                else
                {
                    if (numberOfMsgsSentFromTheAccount == TestMicroservice.maxNumberOfMsgsPerSecondPerAccount)
                    {
                        return StatusCode(StatusCodes.Status417ExpectationFailed, "could not get the number of msgs sent from phone " + phoneNumber);
                    }
                    else
                    {
                        if (!TestMicroservice.numberOfMsgsSentFromPhone.TryGetValue(phoneNumber, out int numberOfMsgsSentFromThePhoneNumber))
                        {
                            return StatusCode(StatusCodes.Status417ExpectationFailed, "could not get the number of msgs sent from phone " + phoneNumber);
                        }
                        else
                        {
                            if (numberOfMsgsSentFromThePhoneNumber == TestMicroservice.maxNumberOfMsgsPerPhoneNumber)
                            {
                                return StatusCode(StatusCodes.Status417ExpectationFailed, "The limit of messages permitted to be send from the phone number " + phoneNumber + " is reached");
                            }
                            else
                            {
                                if (TestMicroservice.numberOfMsgsSentFromPhone.TryUpdate(phoneNumber, numberOfMsgsSentFromThePhoneNumber + 1, numberOfMsgsSentFromThePhoneNumber))
                                {
                                    if (TestMicroservice.numberOfMsgsSentFromAccount.TryUpdate(accountId, numberOfMsgsSentFromTheAccount + 1, numberOfMsgsSentFromTheAccount))
                                    {
                                        //forward the msg to provider.
                                        //Not implemented because the assignment does not specify the method of forwarding (via RESTful call, or via a Message Queue, or what)
                                        return StatusCode(StatusCodes.Status200OK);
                                    }
                                    else
                                    {
                                        //roll back the increment of the number of msgs sent from the phone
                                        if (TestMicroservice.numberOfMsgsSentFromPhone.TryUpdate(phoneNumber, numberOfMsgsSentFromThePhoneNumber, numberOfMsgsSentFromThePhoneNumber + 1))
                                        {
                                            return StatusCode(StatusCodes.Status417ExpectationFailed, "could not increment the number of messages sent by account " + accountId);
                                        }
                                        else
                                        {
                                            return StatusCode(StatusCodes.Status417ExpectationFailed, "could not roll back the number of messages sent by phone " + phoneNumber);
                                        }
                                    }

                                }
                                else
                                {
                                    return StatusCode(StatusCodes.Status417ExpectationFailed, "could not increment the number of messages sent by phone " + phoneNumber);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status501NotImplemented, ex.Message);
            }
        }
    }
}
