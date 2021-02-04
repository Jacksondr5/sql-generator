using System;
using System.Runtime.Serialization;

namespace Core
{
    [Serializable]
    public class InvalidInputException : Exception
    {
        public InvalidInputException(string message) : base(message) { }
        protected InvalidInputException(
            SerializationInfo info,
            StreamingContext context
        ) : base(info, context) { }

        public override void GetObjectData(
            SerializationInfo info,
            StreamingContext context
        )
        {
            base.GetObjectData(info, context);
        }
    }
}