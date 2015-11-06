using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Elmo.Logging;
using Elmo.Responses.Views.Components;
using Elmo.Utilities;
using Microsoft.Owin;

namespace Elmo.Responses.Views
{
    internal class ErrorDetailView : ErrorViewBase
    {
        private ErrorLogEntry errorLogEntry;

        protected override async Task RenderContentsAsync(XmlWriter writer)
        {
            if (errorLogEntry != null)
                await RenderErrorAsync(writer);
            else
                await RenderNoErrorAsync(writer);
        }

        private static async Task RenderNoErrorAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync("p");
            await writer.WriteStringAsync("Error not found in log.");
            await writer.WriteEndElementAsync();
        }

        private async Task RenderErrorAsync(XmlWriter writer)
        {
            var error = errorLogEntry.Error;

            await writer.WriteStartElementAsync("h1");
            await writer.WriteAttributeStringAsync("id", "PageTitle");
            {
                await writer.WriteStringAsync(error.Message);
            }
            await writer.WriteEndElementAsync();

            await SpeedBarComponent.RenderAsync(writer, SpeedBarComponent.GetHome(BasePageName));

            await writer.WriteStartElementAsync("p");
            await writer.WriteAttributeStringAsync("id", "ErrorTitle");
            {
                await writer.WriteStartElementAsync("span");
                await writer.WriteAttributeStringAsync("id", "ErrorType");
                {
                    await writer.WriteStringAsync(error.TypeName);
                }
                await writer.WriteEndElementAsync();

                await writer.WriteStartElementAsync("span");
                await writer.WriteAttributeStringAsync("id", "ErrorTypeMessageSeparator");
                {
                    await writer.WriteStringAsync(": ");
                }
                await writer.WriteEndElementAsync();

                await writer.WriteStartElementAsync("span");
                await writer.WriteAttributeStringAsync("id", "ErrorMessage");
                {
                    await writer.WriteStringAsync(error.Message);
                }
                await writer.WriteEndElementAsync();
            }
            await writer.WriteEndElementAsync();

            await writer.WriteStartElementAsync("pre");
            await writer.WriteAttributeStringAsync("id", "ErrorDetail");
            {
                // TODO: Output exception stacktrace here.
            }
            await writer.WriteEndElementAsync();
        }

        protected override async Task LoadContentsAsync()
        {
            var errorId = OwinContext.Request.Query["id"];

            errorLogEntry = await ErrorLog.GetErrorAsync(errorId);
            if (errorLogEntry == null)
            {
                // TODO: Find a good way to handle not found.
                return;
            }

            PageTitle = $"Error: {errorLogEntry.Error.TypeName} [{errorLogEntry.Id}]";
        }

        public override bool CanProcess(string path)
        {
            return path.StartsWith("/detail");
        }

        public ErrorDetailView(PathString rootPath) : base(rootPath)
        {
        }
    }
}
