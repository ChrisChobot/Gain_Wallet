using Assets.Scripts.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets.Scripts
{
    class SerializableEventableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        private XmlSerializer _keySerializer = new XmlSerializer(typeof(TKey));
        private XmlSerializer _valSerializer = new XmlSerializer(typeof(TValue));

        public delegate void DictionaryChanged(object sender, DictionaryChangedEventArgs<TKey, TValue> e);

        /// <summary>
        /// Triggered when value in dictionary was changed
        /// </summary>
        public event DictionaryChanged OnDictionaryChanged;

        /// <summary>
        /// Filename or path+filename, where (de)serialize data (will be)/are store(d)
        /// </summary>
        public string FileName { get; set; }

        public SerializableEventableDictionary()
        {
        }

        /// <summary>
        /// Construct Serializable Dictionary with events on Add/Remove element with specific filename to (de)serialize dictionary content
        /// </summary>
        /// <param name="filename"></param>
        public SerializableEventableDictionary(string filename)
        {
            FileName = filename;
        }

        /// <summary>
        /// Add new element to dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            OnDictionaryChanged?.Invoke(this, new DictionaryChangedEventArgs<TKey, TValue>(key, value, DictionaryActionType.RemoveItem));
        }

        /// <summary>
        /// Remove element from dictionary
        /// </summary>
        /// <param name="key"></param>
        public new void Remove(TKey key)
        {
            base.Remove(key);
            OnDictionaryChanged?.Invoke(this, new DictionaryChangedEventArgs<TKey, TValue>(key, default, DictionaryActionType.RemoveItem));
        }

        /// <summary>
        /// Serialize with specified in property filename
        /// </summary>
        public void Serialize()
        {
            Serialize(FileName);
        }

        /// <summary>
        /// Serialize dictionary to file
        /// </summary>
        /// <param name="fileName">XML file name (with patch or without)</param>
        public void Serialize(string fileName = "SED.xml")
        {
            using (XmlWriter writer = XmlWriter.Create(fileName))
            {
                writer.WriteStartElement("serialeventdict");
                WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Deserialize with specified in property filename
        /// </summary>
        public void Deserialize()
        {
            Deserialize(FileName);
        }

        /// <summary>
        /// Deerialize dictionary from file (if exists)
        /// </summary>
        /// <param name="fileName">XML file name (with patch or without)</param>
        public void Deserialize(string fileName = "SED.xml")
        {
            if (File.Exists(fileName))
            {
                try
                {
                    using (XmlReader reader = XmlReader.Create(fileName))
                    {
                        if (!reader.IsEmptyElement && reader.IsStartElement())
                        {
                            reader.ReadStartElement("serialeventdict");
                            ReadXml(reader);
                            reader.ReadEndElement();
                        }
                    }
                }
                catch (XmlException ex)
                {
                    Debug.LogWarning($"DESERIALIZATION ERROR - file will be deleted \n{ex}");
                    CopyAndDeleteBrokenFile(fileName);
                }
            }
        }

        /// <summary>
        /// Copy broken file and then delete file (even if copy was failed)
        /// </summary>
        /// <param name="fileName">File name or path+filename</param>
        private void CopyAndDeleteBrokenFile(string fileName)
        {
            try
            {
                string backupFailedFileName = $"{fileName}.{System.DateTime.Now:yyyyMMddHHmmss}.xml";
                Debug.Log(backupFailedFileName);
                File.Copy(fileName, backupFailedFileName);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"COPYING ERROR! \n{ex}");
            }
            finally
            {
                DeleteFile(fileName);
            }
        }

        /// <summary>
        /// Delete selected file
        /// </summary>
        /// <param name="fileName">File name or path+filename</param>
        private void DeleteFile(string fileName)
        {
            try
            {
                File.Delete(fileName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"DELETING FILE ERROR! \n{ex}");
            }
        }

        /// <summary>
        /// Unused method
        /// </summary>
        /// <returns>null value</returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Reading XML inside Dictionary node
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(XmlReader reader)
        {
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("element");

                reader.ReadStartElement("key");
                TKey key = (TKey)_keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                TValue value = (TValue)_valSerializer.Deserialize(reader);
                reader.ReadEndElement();

                base.Add(key, value);

                reader.Read();
            }
        }

        /// <summary>
        /// Write Dictionary to XML file. Root node must be set in writer first.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer)
        {
            foreach (KeyValuePair<TKey, TValue> element in this)
            {
                writer.WriteStartElement("element");

                writer.WriteStartElement("key");
                _keySerializer.Serialize(writer, element.Key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                _valSerializer.Serialize(writer, element.Value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
    }
}
