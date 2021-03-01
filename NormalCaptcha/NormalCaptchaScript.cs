using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using w3bot.Api;
using w3bot.Input;
using w3bot.Script;
using w3bot.Util;

namespace NormalCaptcha
{
	[ScriptManifest("Normal Captcha Example", "Captcha", "This script uses captchas.", "NoChoice", 1.0)]
	public class NormalCaptchaScript : AbstractScript
    {
		private Captcha _captcha;
		private string _response;
		private CaptchaResult _captchaResult;

		public override void OnStart()
		{
			Status.Log("Test script has been started.");

			CreateBrowserWindow(); // create a new browser window

			_captcha = Methods.Captcha; // initialize captcha object

			Browser.Navigate("https://files.w3bot.org/normal-captcha/"); // navigating to website
		}

		public string FetchResponseByTagName(string tagName, string type)
		{
			Task.Run(() =>
			{
				// fetch text of the first element
				var jsData = Browser.ExecuteJavascript("(function() { var result=document.getElementsByTagName('" + tagName + "')[0]." + type + "; return result; })()");

				if (jsData.Result != null)
				{
					// cast the response to a JavascriptResponse object
					var jsResponse = (JavascriptResponse)jsData.Result;

					_response = (string)jsResponse.Result;
				}
			});

			return _response;
		}

		public CaptchaResult FetchCaptchaResult(string jsTextarea)
        {
			Task.Run(() =>
			{
				_captchaResult = _captcha.SolveNormalCaptcha(jsTextarea).Result;
			});

			return _captchaResult;
		}

		public override int OnUpdate()
		{
			// check if browser is ready
			if (Browser.IsReady)
			{
				var jsTextarea = FetchResponseByTagName("textarea", "value");
				if (jsTextarea == "undefined")
				{
					Status.Log("No image found. Stopping script...");
					return -1;
				}

				if (!string.IsNullOrWhiteSpace(jsTextarea))
                {
					// solving normal captcha
					var captchaResult = FetchCaptchaResult(jsTextarea);
					Sleep(5000, 10000);
					Status.Log(captchaResult.Response);
					if (captchaResult.Success)
					{
						var jsTextfield = "document.getElementById('captcha').value = '" + captchaResult.Response + "';";
						var jsButton = "document.getElementsByTagName('button')[0].click()";

						Browser.ExecuteJavascript(jsTextfield);
						Browser.ExecuteJavascript(jsButton);
					}

					if (captchaResult.Response == "ERROR_IMAGE_TYPE_NOT_SUPPORTED")
                    {
						Status.Log("Captcha could not be solved. Image type not supported.");
						return -1;
                    }

					// check response captcha form
					if (!string.IsNullOrWhiteSpace(captchaResult.Response))
                    {
						var jsResult = FetchResponseByTagName("p", "innerText");
						if (!string.IsNullOrWhiteSpace(jsResult))
                        {
							switch (jsResult)
							{
								case "Correct":
									Status.Log("Captcha solved. Stopping script...");
									break;
								case "Wrong":
									Status.Log("Captcha is wrong. Stopping script...");
									break;
							}

							return -1;
						}
					}
				}
			}

			return 1000;
		}

		public override void OnFinish()
		{
			Status.Log("Thank you for using my script.");
		}
	}
}
