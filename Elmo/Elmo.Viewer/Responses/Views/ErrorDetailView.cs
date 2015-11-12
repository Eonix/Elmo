using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Elmo.Logging;
using Elmo.Viewer.Responses.Views.Components;
using Elmo.Viewer.Utilities;
using Microsoft.Owin;

namespace Elmo.Viewer.Responses.Views
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
                await writer.WriteStringAsync(error.Detail);
            }
            await writer.WriteEndElementAsync();

            await writer.WriteStartElementAsync("p");
            await writer.WriteAttributeStringAsync("id", "ErrorLogTime");
            {
                await writer.WriteStringAsync($"Logged on {error.Time}.");
            }
            await writer.WriteEndElementAsync();

            await writer.WriteStartElementAsync("p");
            {
                await writer.WriteStringAsync("See also:");
            }
            await writer.WriteEndElementAsync();

            await writer.WriteStartElementAsync("ul");
            {
                await writer.WriteStartElementAsync("li");
                {
                    await writer.WriteStringAsync("Raw/Source data in ");

                    await writer.WriteStartElementAsync("a");
                    await writer.WriteAttributeStringAsync("href", "json" + OwinContext.Request.Uri.Query);
                    await writer.WriteAttributeStringAsync("rel", "alternate");
                    await writer.WriteAttributeStringAsync("type", "application/json");
                    {
                        await writer.WriteStringAsync("JSON");
                    }
                    await writer.WriteEndElementAsync();
                }
                await writer.WriteEndElementAsync();
            }
            await writer.WriteEndElementAsync();

            await RenderDictionaryAsync(writer, error.ServerEnvironment, "ServerVariables", "Server Environment");
            await RenderDictionaryAsync(writer, error.Headers.ToDictionary(pair => pair.Key, pair => string.Join(", ", pair.Value)), "ServerVariables", "Headers");
            await RenderDictionaryAsync(writer, error.Query.ToDictionary(pair => pair.Key, pair => string.Join(", ", pair.Value)), "ServerVariables", "Query");
            await RenderDictionaryAsync(writer, error.Cookies, "ServerVariables", "Cookies");
        }

        private static async Task RenderDictionaryAsync(XmlWriter writer, Dictionary<string, string> dictionary, string id, string title)
        {
            if (!dictionary.Any())
                return;

            await writer.WriteStartElementAsync("div");
            await writer.WriteAttributeStringAsync("id", id);
            {
                await writer.WriteStartElementAsync("p");
                await writer.WriteAttributeStringAsync("class", "table-caption");
                {
                    await writer.WriteStringAsync(title);
                }
                await writer.WriteEndElementAsync();

                await writer.WriteStartElementAsync("div");
                await writer.WriteAttributeStringAsync("class", "scroll-view");
                {
                    await writer.WriteStartElementAsync("table");
                    {
                        await writer.WriteStartElementAsync("tr");
                        {
                            await writer.WriteStartElementAsync("th");
                            await writer.WriteAttributeStringAsync("class", "name-col");
                            {
                                await writer.WriteStringAsync("Name");
                            }
                            await writer.WriteEndElementAsync();

                            await writer.WriteStartElementAsync("th");
                            await writer.WriteAttributeStringAsync("class", "value-col");
                            {
                                await writer.WriteStringAsync("Value");
                            }
                            await writer.WriteEndElementAsync();
                        }
                        await writer.WriteEndElementAsync();

                        var count = 0;
                        foreach (var line in dictionary)
                        {
                            await writer.WriteStartElementAsync("tr");
                            await writer.WriteAttributeStringAsync("class", count%2 == 0 ? "even-row" : "odd-row");
                            {
                                await writer.WriteStartElementAsync("td");
                                await writer.WriteAttributeStringAsync("class", "key-col");
                                {
                                    await writer.WriteStringAsync(line.Key);
                                }
                                await writer.WriteEndElementAsync();

                                await writer.WriteStartElementAsync("td");
                                await writer.WriteAttributeStringAsync("class", "value-col");
                                {
                                    await writer.WriteStringAsync(line.Value);
                                }
                                await writer.WriteEndElementAsync();
                            }
                            await writer.WriteEndElementAsync();
                        }
                    }
                    await writer.WriteEndElementAsync();
                }
                await writer.WriteEndElementAsync();
            }
            await writer.WriteEndElementAsync();

            await writer.WriteElementStringAsync("br", string.Empty);
        }

        protected override async Task LoadContentsAsync()
        {
            var errorId = OwinContext.Request.Query["id"];

            errorLogEntry = await ErrorLog.GetErrorAsync(errorId);
            if (errorLogEntry == null)
                return;

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
