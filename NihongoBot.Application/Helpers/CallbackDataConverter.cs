
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NihongoBot.Application.Enums;
using NihongoBot.Application.Models;

public class CallbackDataConverter : JsonConverter<ICallbackData>
{
    public override bool CanWrite => false; // We only need to read

    public override ICallbackData ReadJson(JsonReader reader, Type objectType, ICallbackData existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
		// Load the JSON object
		JObject jObject = JObject.Load(reader);
        CallBackType? action = jObject["Type"] != null 
            ? (CallBackType?)Enum.ToObject(typeof(CallBackType), jObject["Type"].Value<int>()) 
            : null;

        ICallbackData target;
        switch (action)
        {
            case CallBackType.ReadyForQuestion:
                target = new ReadyCallbackData();
                break;
            // Add other cases here for different actions/types
            default:
                throw new NotSupportedException($"Action '{action}' is not supported.");
        }

        // Populate the target object with JSON data
        serializer.Populate(jObject.CreateReader(), target);
        return target;
    }

	// We don't need to write JSON, so this method is not implemented
    public override void WriteJson(JsonWriter writer, ICallbackData value, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }
}
