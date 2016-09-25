namespace DataAccess.Models
{
    using System;
    using System.Dynamic;
    using System.Linq;

    using DataAccess.Interfaces;
    using Infrastructure.PureFunctions.Extensions;
    using Newtonsoft.Json;

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

        // apart from keeping an audit trail this is used for sorting 
        public DateTime? Created { get; set; }

        public double? CreatedNumber { get; set; }

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
                this.more = this.more ?? new ExpandoObject();
                return this.more;
            }

            set
            {
                this.more = value;
            }
        }

        [JsonProperty(PropertyName = "schema")]
        public string Schema
        {
            get
            {
                this.schema = this.schema ?? this.GetType().Name;
                return this.schema;
            }

            set
            {
                this.schema = value;
            }
        }

        [JsonProperty(PropertyName = "type")]
        public string Type
        {
            get
            {
                this.type = this.type ?? this.GetType().FullName;
                return this.type;
            }

            set
            {
                this.type = value;
            }
        }

        public void UpdateFromAnotherObject<T>(T source, params string[] exclude)
        {
            if (source == null)
            {
                throw new Exception("Source object is null");
            }

            if (!source.GetType().InheritsOrImplements(this.GetType()))
            {
                throw new Exception("Source object not of the same base type");
            }

            source.CopyProperties(this, exclude.ToArray());
        }
    }
}