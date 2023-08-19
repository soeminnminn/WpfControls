using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Xaml;

namespace wspGridControl
{
    public class GridDocumentRenderer : IDisposable
    {
        #region Variables
        private static readonly NamespaceDeclaration xamlNamespace = new NamespaceDeclaration("http://schemas.microsoft.com/winfx/2006/xaml", "x");

        private readonly GridControl _owner;
        private readonly XamlWriter _writer;
        #endregion

        #region Constructor
        public GridDocumentRenderer(GridControl grid)
        {
            _owner = grid;

            _writer = new XamlObjectWriter(System.Windows.Markup.XamlReader.GetWpfSchemaContext());
        }
        #endregion

        #region Properties
        protected XamlSchemaContext SchemaContext
        {
            get => _writer.SchemaContext;
        }

        protected XamlWriter Writer
        {
            get => _writer;
        }
        #endregion

        #region Methods

        #region Wrapper Methods
        protected virtual XamlType WriteStartObject(Type type)
        {
            var xamlType = SchemaContext.GetXamlType(type);
            _writer.WriteStartObject(xamlType);
            return xamlType;
        }

        protected virtual void WriteEndObject()
        {
            _writer.WriteEndObject();
        }

        protected virtual void WriteObject(Type type, Action<XamlType> writeMembers, Action<XamlType> writeChildren)
        {
            var objectType = SchemaContext.GetXamlType(type);
            _writer.WriteStartObject(objectType);
            if (writeMembers != null)
            {
                writeMembers(objectType);
            }
            if (writeChildren != null)
            {
                writeChildren(objectType);
            }
            _writer.WriteEndObject();
        }

        protected virtual void WriteAttribute(XamlType objectType, string memberName, object value)
        {
            var member = objectType.GetMember(memberName);

            _writer.WriteStartMember(member);
            _writer.WriteValue(value);
            _writer.WriteEndMember();
        }

        protected virtual void WriteAttribute(XamlType objectType, DependencyProperty property, object value)
        {
            if (value == null) return;

            XamlMember member;

            if (property.OwnerType.IsAssignableFrom(objectType.UnderlyingType))
            {
                member = objectType.GetMember(property.Name);
            }
            else
            {
                var type = SchemaContext.GetXamlType(property.OwnerType);
                member = type.GetAttachableMember(property.Name);
            }

            if (_writer is XamlObjectWriter writer)
            {
                writer.WriteStartMember(member);
                writer.WriteValue(value);
                writer.WriteEndMember();
            }
            else
            {
                string strValue;
                if (value is string str)
                    strValue = str;
                else
                {
                    var converter = member.TypeConverter.ConverterInstance;
                    try
                    {
                        strValue = converter.ConvertToString(value);
                    }
                    catch (Exception)
                    {
                        strValue = value.ToString();
                    }
                }

                if (strValue != null)
                {
                    _writer.WriteStartMember(member);
                    _writer.WriteValue(strValue);
                    _writer.WriteEndMember();
                }
            }
        }

        protected virtual void WriteStartCollection(XamlType objectType, string memberName)
        {
            var member = objectType.GetMember(memberName);

            _writer.WriteStartMember(member);
            _writer.WriteGetObject();
            _writer.WriteStartMember(XamlLanguage.Items);
        }

        protected virtual void WriteStartCollection(XamlType objectType, DependencyProperty property)
        {
            XamlMember member;
            if (property.OwnerType.IsAssignableFrom(objectType.UnderlyingType))
                member = objectType.GetMember(property.Name);
            else
            {
                var type = SchemaContext.GetXamlType(property.OwnerType);
                member = type.GetAttachableMember(property.Name);
            }

            _writer.WriteStartMember(member);
            _writer.WriteGetObject();
            _writer.WriteStartMember(XamlLanguage.Items);
        }

        protected virtual void WriteEndCollection()
        {
            _writer.WriteEndMember();
            _writer.WriteEndObject();
            _writer.WriteEndMember();
        }

        protected virtual void WriteCollection(XamlType objectType, string memberName, Action writeChildren)
        {
            var member = objectType.GetMember(memberName);

            _writer.WriteStartMember(member);
            _writer.WriteGetObject();
            _writer.WriteStartMember(XamlLanguage.Items);

            if (writeChildren != null)
            {
                writeChildren();
            }

            _writer.WriteEndMember();
            _writer.WriteEndObject();
            _writer.WriteEndMember();
        }

        protected virtual void WriteCollection(XamlType objectType, DependencyProperty property, Action writeChildren)
        {
            XamlMember member;
            if (property.OwnerType.IsAssignableFrom(objectType.UnderlyingType))
                member = objectType.GetMember(property.Name);
            else
            {
                var type = SchemaContext.GetXamlType(property.OwnerType);
                member = type.GetAttachableMember(property.Name);
            }

            _writer.WriteStartMember(member);
            _writer.WriteGetObject();
            _writer.WriteStartMember(XamlLanguage.Items);

            if (writeChildren != null)
            {
                writeChildren();
            }

            _writer.WriteEndMember();
            _writer.WriteEndObject();
            _writer.WriteEndMember();
        }

        protected virtual void WriteStartContent(XamlType objectType)
        {
            XamlMember member = objectType.ContentProperty;
            _writer.WriteStartMember(member);
            _writer.WriteGetObject();
            _writer.WriteStartMember(XamlLanguage.Items);
        }

        protected virtual void WriteEndContent()
        {
            _writer.WriteEndMember();
            _writer.WriteEndObject();
            _writer.WriteEndMember();
        }

        protected virtual void WriteContent(XamlType objectType, Action writeChildren)
        {
            XamlMember member = objectType.ContentProperty;
            _writer.WriteStartMember(member);
            _writer.WriteGetObject();
            _writer.WriteStartMember(XamlLanguage.Items);

            if (writeChildren != null)
            {
                writeChildren();
            }

            _writer.WriteEndMember();
            _writer.WriteEndObject();
            _writer.WriteEndMember();
        }

        protected virtual void WriteTextRun(string text)
        {
            var xamlType = SchemaContext.GetXamlType(typeof(Run));
            var member = xamlType.GetMember(nameof(Run.Text));

            _writer.WriteStartObject(xamlType);
            _writer.WriteStartMember(member);
            _writer.WriteValue(text);
            _writer.WriteEndMember();
            _writer.WriteEndObject();
        }
        #endregion

        protected virtual void WriteTableColumn(double width)
        {
            var columnType = WriteStartObject(typeof(TableColumn));
            WriteAttribute(columnType, TableColumn.WidthProperty, width);
            WriteEndObject(); // TableColumn
        }

        protected virtual void WriteCell(string text, TextAlignment textAlignment)
        {
            var cellType = WriteStartObject(typeof(TableCell));
            WriteStartContent(cellType); // TableCell.Content

            var paraType = WriteStartObject(typeof(Paragraph));
            WriteAttribute(paraType, Paragraph.PaddingProperty, 4);
            WriteAttribute(paraType, Paragraph.TextAlignmentProperty, textAlignment);

            WriteStartContent(paraType); // Paragraph.Content
            _writer.WriteValue(text);
            WriteEndContent(); // Paragraph.Content

            WriteEndObject(); // Paragraph

            WriteEndContent(); // TableCell.Content
            WriteEndObject(); // TableCell
        }

        protected virtual void WriteHorizontalRule(int columnsCount)
        {
            var rowType = WriteStartObject(typeof(TableRow));
            WriteAttribute(rowType, TableRow.FontSizeProperty, 0.2);
            WriteAttribute(rowType, TableRow.BackgroundProperty, SystemColors.ActiveBorderColor.ToString());

            WriteStartCollection(rowType, nameof(TableRow.Cells));

            var cellType = WriteStartObject(typeof(TableCell));
            WriteAttribute(cellType, TableCell.ColumnSpanProperty, columnsCount);
            WriteEndObject(); // TableCell

            WriteEndCollection(); // TableRow.Cells

            WriteEndObject(); // TableRow
        }

        protected virtual void WriteTable(Column[] columns, IEnumerable<string[]> list)
        {
            var tableType = WriteStartObject(typeof(Table));
            WriteAttribute(tableType, Table.CellSpacingProperty, 0);

            // Table.Columns
            WriteCollection(tableType, nameof(Table.Columns), () =>
            {
                foreach (var column in columns)
                {
                    WriteTableColumn(column.Width);
                }
            });

            WriteStartCollection(tableType, nameof(Table.RowGroups));

            var rowGroupType = WriteStartObject(typeof(TableRowGroup));
            WriteStartCollection(rowGroupType, nameof(TableRowGroup.Rows));

            WriteHorizontalRule(columns.Length);

            var rowType = WriteStartObject(typeof(TableRow)); // [Header]
            WriteAttribute(rowType, TableRow.FontSizeProperty, 14.0);
            WriteAttribute(rowType, TableRow.FontWeightProperty, "SemiBold");

            // TableRow.Cells
            WriteCollection(rowType, nameof(TableRow.Cells), () =>
            {
                foreach (var column in columns)
                {
                    WriteCell(column.Header, column.HeaderAlignment);
                }
            });
            WriteEndObject(); // TableRow [Header]

            WriteHorizontalRule(columns.Length);

            using (var emu = list.GetEnumerator())
            {
                while (emu.MoveNext())
                {
                    var bodyRowType = WriteStartObject(typeof(TableRow));
                    WriteAttribute(bodyRowType, TableRow.FontSizeProperty, 12.0);

                    var cells = emu.Current;

                    // TableRow.Cells
                    WriteCollection(rowType, nameof(TableRow.Cells), () =>
                    {
                        for (int i = 0; i < columns.Length; i++)
                        {
                            if (cells.Length > i)
                            {
                                WriteCell(cells[i], columns[i].CellAlignment);
                            }
                            else
                            {
                                WriteCell(string.Empty, columns[i].CellAlignment);
                            }
                        }
                    });

                    WriteEndObject(); // TableRow
                }
            }

            WriteEndCollection(); // TableRowGroup.Rows
            WriteEndObject(); // TableRowGroup

            WriteEndCollection(); // Table.RowGroups

            WriteEndObject(); // Table
        }

        protected virtual void WriteDocument()
        {
            var columns = BuildColumns(out double totalWidth);

            _writer.WriteNamespace(xamlNamespace);

            var docType = WriteStartObject(typeof(FlowDocument));
            WriteAttribute(docType, FlowDocument.FontFamilyProperty, _owner.FontFamily);
            WriteAttribute(docType, FlowDocument.FontSizeProperty, _owner.FontSize);
            WriteAttribute(docType, FlowDocument.FontStyleProperty, _owner.FontStyle);
            WriteAttribute(docType, FlowDocument.FontWeightProperty, _owner.FontWeight);
            WriteAttribute(docType, FlowDocument.FontStretchProperty, _owner.FontStretch);

            if (columns.Length > 0)
            {
                var list = new GridSourceEnumerable(_owner.GridSource);

                WriteAttribute(docType, FlowDocument.PageWidthProperty, totalWidth);

                WriteStartCollection(docType, nameof(FlowDocument.Blocks));

                WriteTable(columns, list);

                WriteEndCollection(); // FlowDocument.Blocks
            }

            WriteEndObject(); // FlowDocument
        }

        protected virtual Column[] BuildColumns(out double totalWidth)
        {
            var columns = _owner.Columns;
            IGridSource source = _owner.GridSource;

            totalWidth = 0.0;
            if (columns.Count == 0 || source == null) return new Column[0];

            int columnsCount = columns.Count;
            long rowsCount = source.RowsCount;
            double avCharWidth = _owner.AverageCharWidth;
            var indexes = columns.IndexList;

            int[] maxTextLengths = new int[columnsCount];
            if (avCharWidth > 0)
            {
                for (long r = 0; r < rowsCount; r++)
                {
                    for (int c = 0; c < columnsCount; c++)
                    {
                        var colIdx = indexes[c];
                        string value = source.GetCellDataAsString(r, colIdx);
                        if (string.IsNullOrEmpty(value)) continue;
                        maxTextLengths[c] = Math.Max(maxTextLengths[c], value.Length);
                    }
                }
            }

            var result = new Column[columnsCount];

            for (int i = 0; i < columnsCount; i++)
            {
                var column = columns[i];
                var col = new Column()
                {
                    HeaderAlignment = column.HeaderTextAlignment,
                    CellAlignment = column.CellTextAlignment
                };

                if (avCharWidth == 0.0)
                {
                    col.Width = column.FinalWidth;
                }
                else
                {
                    col.Width = maxTextLengths[i] * avCharWidth;
                }

                string stringFormat = column.HeaderStringFormat;
                if (string.IsNullOrEmpty(stringFormat))
                    stringFormat = "{0}";

                var header = _owner.ContentMgr.GetColumnHeaderContent(column.ActualIndex);
                col.Header = header == null ? string.Empty : string.Format(stringFormat, header);

                totalWidth += col.Width; 
                result[i] = col;
            }

            return result;
        }

        public virtual void Dispose()
        {
            _writer.Close();
        }

        public virtual FlowDocument ToFlowDocument()
        {
            WriteDocument();

            if (_writer is XamlObjectWriter ow)
            {
                return ow.Result as FlowDocument;
            }
            return null;
        }

        #region Save Methods
        public void Save(TextWriter textWriter)
        {
            IDocumentPaginatorSource documentSource = ToFlowDocument();
            if (documentSource != null)
            {
                System.Windows.Markup.XamlWriter.Save(documentSource, textWriter);
            }
        }

        public void Save(System.Xml.XmlWriter xmlWriter)
        {
            IDocumentPaginatorSource documentSource = ToFlowDocument();
            if (documentSource != null)
            {
                System.Windows.Markup.XamlWriter.Save(documentSource, xmlWriter);
            }
        }

        public void Save(Stream stream)
        {
            IDocumentPaginatorSource documentSource = ToFlowDocument();
            if (documentSource != null)
            {
                System.Windows.Markup.XamlWriter.Save(documentSource, stream);
            }
        }
        #endregion

        #endregion

        #region Nested Types
        public struct Column
        {
            public string Header;
            public double Width;
            public TextAlignment HeaderAlignment;
            public TextAlignment CellAlignment;
        }
        #endregion
    }
}
