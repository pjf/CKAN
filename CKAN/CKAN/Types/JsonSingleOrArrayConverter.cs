using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace CKAN
{

    // With thanks to 
    // https://stackoverflow.com/questions/18994685/how-to-handle-both-a-single-item-and-an-array-for-the-same-property-using-json-n

    public class JsonSingleOrArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type object_type)
        {
            return(object_type == typeof(List<T>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);

            // If we have an array, convert it to a list of things.
            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<T>>();
            }

            // If the object is null, we'll return an empty list.
            // This also means we have defined by empty lists for stanzas which weren't defined,
            // which is almost alays what we want.
            // if (token.ToObject<T>() == null)
            // {
            //    return new List<T> ();
            // }

            // Otherwise, return the thing wrapped in a list.
            return new List<T> { token.ToObject<T>() };
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}

