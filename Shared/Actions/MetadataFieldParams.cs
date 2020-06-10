﻿namespace CloudinaryDotNet.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents the base definition of a single metadata field.
    /// Use one of the derived classes in the metadata API calls.
    /// </summary>
    /// <typeparam name="T">Type that can describe the field type.</typeparam>
    public abstract class MetadataFieldBaseParams<T> : BaseParams
    {
        /// <summary>
        /// Gets or sets an optional unique immutable identification string for the metadata field.
        /// Default: auto-generated by Cloudinary, although it is recommended to specify this.
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// Gets or sets the type of value that can be assigned to the metadata field.
        /// </summary>
        public MetadataFieldType Type { get; set; }

        /// <summary>
        /// Gets or sets the label of the metadata field for display purposes.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a value must be given for this field, either when an asset is first uploaded,
        /// or when it is updated. Default: false.
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// Gets or sets the default value for the field (a set can have multiple default values defined by an array).
        /// Default: null. Mandatory, if 'mandatory' is true.
        /// </summary>
        public T DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets any validation rules to apply when values are entered (or updated) for this field.
        /// </summary>
        public MetadataValidationParams Validation { get; set; }

        /// <summary>
        /// Gets or sets the predefined list of values, referenced by external_ids, available for this field.
        /// The datasource parameter is only relevant for fields where the selected values must come
        /// from a predefined list of values ('enum' or 'set' type fields).
        /// </summary>
        public MetadataDataSourceParams DataSource { get; set; }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            AddParam(dict, "type", Api.GetCloudinaryParam(Type));
            AddParam(dict, "mandatory", Mandatory);

            if (!string.IsNullOrEmpty(ExternalId))
            {
                AddParam(dict, "external_id", ExternalId);
            }

            if (Validation != null)
            {
                dict.Add("validation", Validation.ToParamsDictionary());
            }

            if (DataSource != null)
            {
                dict.Add("datasource", DataSource.ToParamsDictionary());
            }
        }

        /// <summary>
        /// Validate object models for instances with validation.
        /// </summary>
        /// <param name="allowedValidationTypes">List of validation types allowed for the metadata field type.</param>
        protected void CheckScalarDataModel(List<Type> allowedValidationTypes)
        {
            ShouldNotBeSpecified(() => DataSource);

            if (Validation == null)
            {
                return;
            }

            var validationType = Validation.GetType();
            var hasForbiddenValidationRule = !allowedValidationTypes.Contains(validationType);
            var allowedTypeNames = string.Join(", ", allowedValidationTypes.Select(type => type.Name));
            if (hasForbiddenValidationRule)
            {
                var message = $"Only validations of types {allowedTypeNames} can be applied to the metadata field";
                throw new ArgumentException(message);
            }

            Validation.Check();
        }
    }

    /// <summary>
    /// Represents parameters, required for metadata field creation.
    /// </summary>
    /// <typeparam name="T">Type that can describe the field type.</typeparam>
    public abstract class MetadataFieldCreateParams<T> : MetadataFieldBaseParams<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataFieldCreateParams{T}"/> class.
        /// </summary>
        /// <param name="label">The label of the metadata field.</param>
        protected MetadataFieldCreateParams(string label)
        {
            Label = label;
        }

        /// <summary>
        /// Validates object model.
        /// </summary>
        public override void Check()
        {
            ShouldBeSpecified(() => Label);

            if (Mandatory)
            {
                ShouldBeSpecified(() => DefaultValue);
            }
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            AddParam(dict, "label", Label);
        }
    }

    /// <summary>
    /// Represents parameters, required for metadata field update.
    /// </summary>
    /// <typeparam name="T">Type that can describe the field type.</typeparam>
    public abstract class MetadataFieldUpdateParams<T> : MetadataFieldBaseParams<T>
    {
        /// <summary>
        /// Validates object model.
        /// </summary>
        public override void Check()
        {
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            if (!string.IsNullOrEmpty(Label))
            {
                AddParam(dict, "label", Label);
            }
        }
    }

    /// <summary>
    /// Represents parameters, required for 'integer' metadata field creation.
    /// </summary>
    public class IntMetadataFieldCreateParams : MetadataFieldCreateParams<int?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntMetadataFieldCreateParams"/> class.
        /// </summary>
        /// <param name="label">The label of the metadata field.</param>
        public IntMetadataFieldCreateParams(string label)
            : base(label)
        {
            Type = MetadataFieldType.Integer;
        }

        /// <summary>
        /// Validates object model.
        /// </summary>
        public override void Check()
        {
            base.Check();
            var allowedValidationTypes = new List<Type>
            {
                typeof(IntLessThanValidationParams),
                typeof(IntGreaterThanValidationParams),
                typeof(AndValidationParams),
            };
            CheckScalarDataModel(allowedValidationTypes);
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            if (DefaultValue != null)
            {
                dict.Add("default_value", DefaultValue.Value);
            }
        }
    }

    /// <summary>
    /// Represents parameters, required for 'integer' metadata field update.
    /// </summary>
    public class IntMetadataFieldUpdateParams : MetadataFieldUpdateParams<int?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntMetadataFieldUpdateParams"/> class.
        /// </summary>
        public IntMetadataFieldUpdateParams()
        {
            Type = MetadataFieldType.Integer;
        }

        /// <summary>
        /// Validates object model.
        /// </summary>
        public override void Check()
        {
            base.Check();
            var allowedValidationTypes = new List<Type>
            {
                typeof(IntLessThanValidationParams),
                typeof(IntGreaterThanValidationParams),
                typeof(AndValidationParams),
            };
            CheckScalarDataModel(allowedValidationTypes);
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            if (DefaultValue != null)
            {
                dict.Add("default_value", DefaultValue.Value);
            }
        }
    }

    /// <summary>
    /// Represents parameters, required for 'string' metadata field creation.
    /// </summary>
    public class StringMetadataFieldCreateParams : MetadataFieldCreateParams<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringMetadataFieldCreateParams"/> class.
        /// </summary>
        /// <param name="label">The label of the metadata field.</param>
        public StringMetadataFieldCreateParams(string label)
            : base(label)
        {
            Type = MetadataFieldType.String;
        }

        /// <summary>
        /// Validates object model.
        /// </summary>
        public override void Check()
        {
            base.Check();
            var allowedValidationTypes = new List<Type>
            {
                typeof(StringLengthValidationParams),
                typeof(AndValidationParams),
            };
            CheckScalarDataModel(allowedValidationTypes);
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            if (DefaultValue != null)
            {
                dict.Add("default_value", DefaultValue);
            }
        }
    }

    /// <summary>
    /// Represents parameters, required for 'string' metadata field update.
    /// </summary>
    public class StringMetadataFieldUpdateParams : MetadataFieldUpdateParams<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringMetadataFieldUpdateParams"/> class.
        /// </summary>
        public StringMetadataFieldUpdateParams()
        {
            Type = MetadataFieldType.String;
        }

        /// <summary>
        /// Validates object model.
        /// </summary>
        public override void Check()
        {
            base.Check();
            var allowedValidationTypes = new List<Type>
            {
                typeof(StringLengthValidationParams),
                typeof(AndValidationParams),
            };
            CheckScalarDataModel(allowedValidationTypes);
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            if (DefaultValue != null)
            {
                dict.Add("default_value", DefaultValue);
            }
        }
    }

    /// <summary>
    /// Represents parameters, required for 'date' metadata field creation.
    /// </summary>
    public class DateMetadataFieldCreateParams : MetadataFieldCreateParams<DateTime?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateMetadataFieldCreateParams"/> class.
        /// </summary>
        /// <param name="label">The label of the metadata field.</param>
        public DateMetadataFieldCreateParams(string label)
            : base(label)
        {
            Type = MetadataFieldType.Date;
        }

        /// <summary>
        /// Validates object model.
        /// </summary>
        public override void Check()
        {
            base.Check();
            var allowedValidationTypes = new List<Type>
            {
                typeof(DateGreaterThanValidationParams),
                typeof(DateLessThanValidationParams),
                typeof(AndValidationParams),
            };
            CheckScalarDataModel(allowedValidationTypes);
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            if (DefaultValue != null)
            {
                AddParam(dict, "default_value", DefaultValue.Value.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture));
            }
        }
    }

    /// <summary>
    /// Represents parameters, required for 'date' metadata field update.
    /// </summary>
    public class DateMetadataFieldUpdateParams : MetadataFieldUpdateParams<DateTime?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateMetadataFieldUpdateParams"/> class.
        /// </summary>
        public DateMetadataFieldUpdateParams()
        {
            Type = MetadataFieldType.Date;
        }

        /// <summary>
        /// Validates object model.
        /// </summary>
        public override void Check()
        {
            base.Check();
            var allowedValidationTypes = new List<Type>
            {
                typeof(DateGreaterThanValidationParams),
                typeof(DateLessThanValidationParams),
                typeof(AndValidationParams),
            };
            CheckScalarDataModel(allowedValidationTypes);
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            if (DefaultValue != null)
            {
                AddParam(dict, "default_value", DefaultValue.Value.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture));
            }
        }
    }

    /// <summary>
    /// Represents parameters, required for 'enum' metadata field creation.
    /// </summary>
    public class EnumMetadataFieldCreateParams : MetadataFieldCreateParams<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumMetadataFieldCreateParams"/> class.
        /// </summary>
        /// <param name="label">The label of the metadata field.</param>
        public EnumMetadataFieldCreateParams(string label)
            : base(label)
        {
            Type = MetadataFieldType.Enum;
        }

        /// <summary>
        /// Validates object model.
        /// </summary>
        public override void Check()
        {
            base.Check();
            ShouldBeSpecified(() => DataSource);
            ShouldNotBeSpecified(() => Validation);
            DataSource.Check();
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            if (DefaultValue != null)
            {
                AddParam(dict, "default_value", DefaultValue);
            }
        }
    }

    /// <summary>
    /// Represents parameters, required for 'enum' metadata field update.
    /// </summary>
    public class EnumMetadataFieldUpdateParams : MetadataFieldUpdateParams<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumMetadataFieldUpdateParams"/> class.
        /// </summary>
        public EnumMetadataFieldUpdateParams()
        {
            Type = MetadataFieldType.Enum;
        }

        /// <summary>
        /// Validate object model.
        /// </summary>
        public override void Check()
        {
            base.Check();
            DataSource?.Check();
            ShouldNotBeSpecified(() => Validation);
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            if (DefaultValue != null)
            {
                AddParam(dict, "default_value", DefaultValue);
            }
        }
    }

    /// <summary>
    /// Represents parameters, required for 'set' metadata field creation.
    /// </summary>
    public class SetMetadataFieldCreateParams : MetadataFieldCreateParams<List<string>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetMetadataFieldCreateParams"/> class.
        /// </summary>
        /// <param name="label">The label of the metadata field.</param>
        public SetMetadataFieldCreateParams(string label)
            : base(label)
        {
            Type = MetadataFieldType.Set;
        }

        /// <summary>
        /// Validates object model.
        /// </summary>
        public override void Check()
        {
            base.Check();

            if (Mandatory)
            {
                ShouldNotBeEmpty(() => DefaultValue);
            }

            ShouldBeSpecified(() => DataSource);
            ShouldNotBeSpecified(() => Validation);
            DataSource.Check();
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            if (DefaultValue != null)
            {
                AddParam(dict, "default_value", DefaultValue);
            }
        }
    }

    /// <summary>
    /// Represents parameters, required for 'set' metadata field update.
    /// </summary>
    public class SetMetadataFieldUpdateParams : MetadataFieldUpdateParams<List<string>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetMetadataFieldUpdateParams"/> class.
        /// </summary>
        public SetMetadataFieldUpdateParams()
        {
            Type = MetadataFieldType.Set;
        }

        /// <summary>
        /// Validates object model.
        /// </summary>
        public override void Check()
        {
            base.Check();
            DataSource?.Check();
            ShouldNotBeSpecified(() => Validation);
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            if (DefaultValue != null)
            {
                AddParam(dict, "default_value", DefaultValue);
            }
        }
    }

    /// <summary>
    /// Represents a data source for a given field. This is used in both 'Set' and 'Enum' field types.
    /// The datasource holds a list of the valid values to be used with the corresponding metadata field.
    /// </summary>
    public class MetadataDataSourceParams : BaseParams
    {
        /// <summary>
        /// Gets or sets a list of datasource values.
        /// </summary>
        public List<EntryParams> Values { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataDataSourceParams"/> class.
        /// </summary>
        /// <param name="entries">Datasource values.</param>
        public MetadataDataSourceParams(List<EntryParams> entries)
        {
            Values = entries;
        }

        /// <summary>
        /// Validates object model.
        /// </summary>
        public override void Check()
        {
            ShouldNotBeEmpty(() => Values);
            Values.ForEach(value => value.Check());
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            var valuesList = Values.Select(entry => entry.ToParamsDictionary()).ToList();
            dict.Add("values", valuesList);
        }
    }

    /// <summary>
    /// Defines a single possible value for the field.
    /// </summary>
    public class EntryParams : BaseParams
    {
        /// <summary>
        /// Gets or sets a unique immutable identification string for the datasource entry,
        /// (required if the value is referenced by the default_value field).
        /// Default: auto-generated by Cloudinary (optional).
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// Gets or sets the value for this datasource.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntryParams"/> class.
        /// </summary>
        /// <param name="value">The datasource value.</param>
        public EntryParams(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntryParams"/> class.
        /// </summary>
        /// <param name="value">The datasource value.</param>
        /// <param name="externalId">Unique identifier of the datasource entry.</param>
        public EntryParams(string value, string externalId)
        {
            ExternalId = externalId;
            Value = value;
        }

        /// <summary>
        /// Validates object model.
        /// </summary>
        public override void Check()
        {
            ShouldNotBeEmpty(() => Value);
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            dict.Add("value", Value);
            if (!string.IsNullOrEmpty(ExternalId))
            {
                dict.Add("external_id", ExternalId);
            }
        }
    }

    /// <summary>
    /// Represents the base class for metadata fields validation mechanisms.
    /// </summary>
    public abstract class MetadataValidationParams : BaseParams
    {
        /// <summary>
        /// Gets or sets the type of value that can be assigned to the metadata field.
        /// </summary>
        public MetadataValidationType Type { get; set; }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            AddParam(dict, "type", Api.GetCloudinaryParam(Type));
        }
    }

    /// <summary>
    /// Strlen validation, relevant to 'string' field types only.
    /// </summary>
    public class StringLengthValidationParams : MetadataValidationParams
    {
        /// <summary>
        /// Gets or sets the minimum string length, represented by a positive integer.
        /// Default value: 0.
        /// </summary>
        public int? Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum string length, represented by a positive integer.
        /// Default value: 1024.
        /// </summary>
        public int? Max { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringLengthValidationParams"/> class.
        /// </summary>
        public StringLengthValidationParams()
        {
            Type = MetadataValidationType.StringLength;
        }

        /// <summary>
        /// Validates object model.
        /// Either min or max must be given, supplying both is optional.
        /// </summary>
        public override void Check()
        {
            if (Min == null && Max == null)
            {
                throw new ArgumentException("Either Min or Max must be specified");
            }

            if (Min != null && Min.Value < 0)
            {
                throw new ArgumentException("Min must be a positive integer");
            }

            if (Max != null && Max.Value < 0)
            {
                throw new ArgumentException("Max must be a positive integer");
            }
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            if (Min != null)
            {
                dict.Add("min", Min.Value);
            }

            if (Max != null)
            {
                dict.Add("max", Max.Value);
            }
        }
    }

    /// <summary>
    /// Base class for comparison validations.
    /// </summary>
    /// <typeparam name="T">Type that can describe the value for validation.</typeparam>
    public abstract class ComparisonValidationParams<T> : MetadataValidationParams
    {
        /// <summary>
        /// Gets or sets the value for validation.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to check if equals.
        /// Default value: false.
        /// </summary>
        public bool IsEqual { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonValidationParams{T}"/> class.
        /// </summary>
        /// <param name="value">Value that will be used to compare with.</param>
        protected ComparisonValidationParams(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Validates object model.
        /// </summary>
        public override void Check()
        {
            ShouldBeSpecified(() => Value);
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            AddParam(dict, "equals", IsEqual);
        }
    }

    /// <summary>
    /// Greater-than rule for integers.
    /// </summary>
    public class IntGreaterThanValidationParams : ComparisonValidationParams<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntGreaterThanValidationParams"/> class.
        /// </summary>
        /// <param name="value">Value that will be used to compare with.</param>
        public IntGreaterThanValidationParams(int value)
            : base(value)
        {
            Type = MetadataValidationType.GreaterThan;
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            dict.Add("value", Value);
        }
    }

    /// <summary>
    /// Greater-than rule for dates.
    /// </summary>
    public class DateGreaterThanValidationParams : ComparisonValidationParams<DateTime>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateGreaterThanValidationParams"/> class.
        /// </summary>
        /// <param name="value">Value that will be used to compare with.</param>
        public DateGreaterThanValidationParams(DateTime value)
            : base(value)
        {
            Type = MetadataValidationType.GreaterThan;
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            dict.Add("value", Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// Less-than rule for integers.
    /// </summary>
    public class IntLessThanValidationParams : ComparisonValidationParams<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntLessThanValidationParams"/> class.
        /// </summary>
        /// <param name="value">Value that will be used to compare with.</param>
        public IntLessThanValidationParams(int value)
            : base(value)
        {
            Type = MetadataValidationType.LessThan;
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            dict.Add("value", Value);
        }
    }

    /// <summary>
    /// Less-than rule for dates.
    /// </summary>
    public class DateLessThanValidationParams : ComparisonValidationParams<DateTime>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateLessThanValidationParams"/> class.
        /// </summary>
        /// <param name="value">Value that will be used to compare with.</param>
        public DateLessThanValidationParams(DateTime value)
            : base(value)
        {
            Type = MetadataValidationType.LessThan;
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            dict.Add("value", Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// And validation, relevant for all field types.
    /// Allows to include more than one validation rule to be evaluated.
    /// </summary>
    public class AndValidationParams : MetadataValidationParams
    {
        /// <summary>
        /// Gets or sets rules combined with an 'AND' logic relation between them.
        /// </summary>
        public List<MetadataValidationParams> Rules { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AndValidationParams"/> class.
        /// </summary>
        /// <param name="rules">List of combined rules.</param>
        public AndValidationParams(List<MetadataValidationParams> rules)
        {
            Type = MetadataValidationType.And;
            Rules = rules;
        }

        /// <summary>
        /// Validate object model.
        /// </summary>
        public override void Check()
        {
            ShouldNotBeEmpty(() => Rules);
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            var rulesList = Rules.Select(entry => entry.ToParamsDictionary()).ToList();
            dict.Add("rules", rulesList);
        }
    }

    /// <summary>
    /// Possible value types that can be assigned to the metadata field.
    /// </summary>
    public enum MetadataFieldType
    {
        /// <summary>
        /// A single string value
        /// </summary>
        [EnumMember(Value = "string")]
        String,

        /// <summary>
        /// A single integer value.
        /// </summary>
        [EnumMember(Value = "integer")]
        Integer,

        /// <summary>
        /// A custom date in the following format: {yyyy-mm-dd}.
        /// </summary>
        [EnumMember(Value = "date")]
        Date,

        /// <summary>
        /// A single value referenced by an 'external_id' from a given list,
        /// predefined with the 'datasource' parameter.
        /// </summary>
        [EnumMember(Value = "enum")]
        Enum,

        /// <summary>
        /// Multiple values referenced by 'external_ids' from a given list,
        /// predefined with the 'datasource' parameter.
        /// </summary>
        [EnumMember(Value = "set")]
        Set,
    }

    /// <summary>
    /// Possible value types that can be assigned to the metadata validation.
    /// </summary>
    public enum MetadataValidationType
    {
        /// <summary>
        /// Greater than validation type.
        /// </summary>
        [EnumMember(Value = "greater_than")]
        GreaterThan,

        /// <summary>
        /// Less than validation type.
        /// </summary>
        [EnumMember(Value = "less_than")]
        LessThan,

        /// <summary>
        /// String length validation type.
        /// </summary>
        [EnumMember(Value = "strlen")]
        StringLength,

        /// <summary>
        /// And validation type.
        /// </summary>
        [EnumMember(Value = "and")]
        And,
    }

    /// <summary>
    /// Represents delete datasource entries operation.
    /// </summary>
    public class DataSourceEntriesParams : BaseParams
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceEntriesParams"/> class.
        /// </summary>
        /// <param name="externalIds">IDs of datasource entries to delete.</param>
        public DataSourceEntriesParams(List<string> externalIds)
        {
            ExternalIds = externalIds;
        }

        /// <summary>
        /// Gets or sets an array of IDs of datasource entries to delete.
        /// </summary>
        public List<string> ExternalIds { get; set; }

        /// <summary>
        /// Validates object model.
        /// </summary>
        public override void Check()
        {
            ShouldNotBeEmpty(() => ExternalIds);
        }

        /// <summary>
        /// Add parameters to the object model dictionary.
        /// </summary>
        /// <param name="dict">Dictionary to be updated with parameters.</param>
        protected override void AddParamsToDictionary(SortedDictionary<string, object> dict)
        {
            base.AddParamsToDictionary(dict);
            AddParam(dict, "external_ids", ExternalIds);
        }
    }
}
