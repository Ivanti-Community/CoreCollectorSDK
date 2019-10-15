// ***************************************************************
// Copyright 2018 Ivanti Inc. All rights reserved.
// ***************************************************************

namespace Collector.SDK.Collectors
{
    public class CollectorConstants
    {
        public static readonly string KEY_STATE = "State";
        public static readonly string KEY_FILENAME = "FileName";
        public static readonly string KEY_FOLDER = "FolderPath";
        public static readonly string KEY_FILEFILTER = "FileFilter";

        public static readonly string STATE_READER_DONE = "Reader Done";
        public static readonly string STATE_READER_ERROR = "Reader Error";

        public static readonly string STATE_TRANSFORMER_DONE = "Transformer Done";
        public static readonly string STATE_TRANSFORMER_ERROR = "Transformer Error";

        public static readonly string STATE_PUBLISHER_DONE = "Publisher Done";
        public static readonly string STATE_PUBLISHER_ERROR = "Publisher Error";
        public static readonly string STATE_UPDATE_COLLECTOR_STATUS = "Update Collector Status";
    }
}
