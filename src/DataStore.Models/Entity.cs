namespace DataStore.Models
{
    using System;
    using System.Dynamic;
    using System.Linq;
    using Interfaces;
    using Newtonsoft.Json;
    using PureFunctions.Extensions;

    /// <summary>
    ///     represents an objects with a unique id
    /// </summary>
    public class Entity : IEntity
    {
        /* Magic More Property is there to allow you add extra data to model's object 
         * for read purposes before passing it to API clients and on API calls inwards. */
        private dynamic more;

        private string schema;

        private string type;

        //here for easier comparison in some systems such as docdb
        public double? CreatedNumber { get; set; }

        // apart from keeping an audit trail this is used for sorting 
        public DateTime? Created { get; set; }

        // this is here to give references which are stored in a models json a unique Id which is necessary during updates to determines 
        // what changes have occurred. It can either be implemented as-is or the getter can be overridden to select another existing property as the key
        public Guid id { get; set; }

        // we require this field to determine if the entities have been updated so we don't
        // have to compare or update every one but only those that have changed.
        public DateTime? Modified { get; set; }

        public dynamic More
        {
            get
            {
                more = more ?? new ExpandoObject();
                return more;
            }

            set { more = value; }
        }

        [JsonProperty(PropertyName = "schema")]
        public string Schema
        {
            get
            {
                schema = schema ?? GetType().Name;
                return schema;
            }

            set { schema = value; }
        }

        [JsonProperty(PropertyName = "type")]
        public string Type
        {
            get
            {
                type = type ?? GetType().FullName;
                return type;
            }

            set { type = value; }
        }

        public void UpdateFromAnotherObject<T>(T source, params string[] exclude)
        {
            if (source == null)
                throw new Exception("Source object is null");

            if (!source.GetType().InheritsOrImplements(GetType()))
                throw new Exception("Source object not of the same base type");

            source.CopyProperties(this, exclude.ToArray());
        }
    }
}