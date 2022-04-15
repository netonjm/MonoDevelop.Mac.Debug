using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonoDevelop.Inspector.Tests
{

    [TestFixture()]
    public class FigmaTest
    {
        const string API_ENDPOINT = "https://api.figma.com/v1";
        public string personal_access_token = "TOKEN";

        [TestCase("FILE_ID")]
        public void BasicParsing_Test(string file)
        {
            //var response = WebHelpers.GetWebApiResponse(file, personal_access_token);
            //Assert.IsTrue(response.document.children.Length > 0);
            //var resultNodes = new List<FigmaNode>();
            //response.document.children.Recursively("2.1. Get to Code – Sign In – Option A", resultNodes);
            //var figmaFrame = resultNodes.FirstOrDefault () as FigmaFrameEntity; //window
            //var id = new string[] { "14:679", "14:682" };
            //var bytes = FigmaQueryHelper.GetFigmaImage(file, id, personal_access_token);
            //2398b34723f97b907467c04b6945eaff37c16608Bitmap.fr
            //using (Bitmap image = Image.FromStream(new MemoryStream(bytes)))
            //{
            //    image.Save("output.jpg", ImageFormat.Jpeg);  // Or Png
            //}

           // Assert.NotNull(bytes);
        }
    }
}
