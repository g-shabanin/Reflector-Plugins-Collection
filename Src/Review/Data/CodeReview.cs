namespace Reflector.Review.Data
{
	using System;
	using System.Collections;
	using System.Globalization;
	using System.Xml;
	using System.Xml.XPath;
	using Reflector.CodeModel;

	[Serializable]
    internal sealed class CodeReview
    {
		private const string DateTimeFormatString = "u";
		private readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en-US");
		
		private readonly IList annotations = new ArrayList(1);

		public IList Annotations
        {
            get 
			{ 
				return this.annotations; 
			}
        }

        public int FindAnnotation(object value)
        {
            CodeIdentifier valueIdentifier = new CodeIdentifier(value);

            int index = 0;
            foreach (CodeAnnotation annotation in this.Annotations)
            {
                if (annotation.Identifier.Identifier == valueIdentifier.Identifier)
                    return index;
                index++;
            }
            return -1;
        }

        public void Merge(CodeReview[] reviews)
        {
			if (reviews == null)
			{
				throw new ArgumentNullException("reviews");
			}

            // build hash of current annontations
            Hashtable map = new Hashtable(this.Annotations.Count);
			foreach (CodeAnnotation annotation in this.Annotations)
			{
				map.Add(annotation.Identifier.Identifier, annotation);
			}

            foreach (CodeReview review in reviews)
            {
                foreach (CodeAnnotation annontationToMerge in review.Annotations)
                {
                    // find existing annotation
                    CodeAnnotation existing = (CodeAnnotation)map[annontationToMerge.Identifier.Identifier];
                    if (existing == null)
                    {
                        this.Annotations.Add(annontationToMerge);
                        map.Add(annontationToMerge.Identifier.Identifier, annontationToMerge);
                    }
                    else
                    {
                        // merge changes in existing annotation
                        foreach (CodeChange change in annontationToMerge.Changes)
                            existing.Changes.Add(change);
                    }
                }
            }

            // finally sort changes
			foreach (CodeAnnotation annotation in this.Annotations)
			{
				annotation.Update();
			}
        }

		public void Load(XmlReader reader)
		{
			XPathDocument document = new XPathDocument(reader);
			XPathNodeIterator nav = document.CreateNavigator().Select("Review/Annotation");
			while (nav.MoveNext())
			{
				CodeAnnotation annotation = new CodeAnnotation();
				annotation.Identifier = new CodeIdentifier(nav.Current.GetAttribute("CodeUri", ""));

				XPathNodeIterator cnav = nav.Current.Select("Change");
				while (cnav.MoveNext())
				{
					CodeChange change = new CodeChange();
					change.ChangedBy = cnav.Current.GetAttribute("ChangedBy", "");
					change.ChangedDate = DateTime.ParseExact(cnav.Current.GetAttribute("ChangedDate", ""), DateTimeFormatString, Culture);
					change.Status = (CodeAnnotationStatus)Enum.Parse(typeof(CodeAnnotationStatus), cnav.Current.GetAttribute("Status", ""));
					change.Resolution = (CodeAnnotationResolution)Enum.Parse(typeof(CodeAnnotationResolution), cnav.Current.GetAttribute("Resolution", ""));
					XPathNodeIterator comnav = cnav.Current.Select("Comment");
					while (comnav.MoveNext())
					{
						change.Comment = comnav.Current.Value;
						break;
					}
					XPathNodeIterator fnav = cnav.Current.Select("ChangedField");
					while (fnav.MoveNext())
					{
						CodeChangedField changedField = new CodeChangedField(fnav.Current.GetAttribute("Name", ""), fnav.Current.GetAttribute("Value", ""));
						change.ChangedFields.Add(changedField);
					}
					annotation.Changes.Add(change);
				}

				annotation.Update();
				this.Annotations.Add(annotation);
			}
		}

		public void Save(XmlWriter writer)
		{
			writer.WriteStartElement("Review");
			{
				foreach (CodeAnnotation annotation in this.Annotations)
				{
					writer.WriteStartElement("Annotation");
					{
						writer.WriteAttributeString("CodeUri", annotation.Identifier.Identifier);
						if (annotation.HasChanges)
						{
							writer.WriteAttributeString("CreatedBy", annotation.CreatedBy);
							writer.WriteAttributeString("CreatedDate", annotation.CreatedDate.ToString("u"));
							writer.WriteAttributeString("ChangedBy", annotation.ChangedBy);
							writer.WriteAttributeString("ChangedDate", annotation.ChangedDate.ToString("u"));
							writer.WriteAttributeString("Status", annotation.Status.ToString());
							writer.WriteAttributeString("Resolution", annotation.Resolution.ToString());
						}
						foreach (CodeChange change in annotation.Changes)
						{
							writer.WriteStartElement("Change");
							{
								writer.WriteAttributeString("ChangedBy", change.ChangedBy);
								writer.WriteAttributeString("ChangedDate", change.ChangedDate.ToString("u"));
								writer.WriteAttributeString("Status", change.Status.ToString());
								writer.WriteAttributeString("Resolution", change.Resolution.ToString());
								writer.WriteStartElement("Comment");
								{
									writer.WriteCData(change.Comment);
								}
								writer.WriteEndElement();
								foreach (CodeChangedField changedField in change.ChangedFields)
								{
									writer.WriteStartElement("ChangedField");
									{
										writer.WriteAttributeString("Name", changedField.Name.ToString());
										writer.WriteAttributeString("Value", changedField.Value);
									}
									writer.WriteEndElement();
								}
							}
							writer.WriteEndElement();
						}
					}
					writer.WriteEndElement();
				}
			}
			writer.WriteEndElement();
		}
	}
}
