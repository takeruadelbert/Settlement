namespace Settlement.Classes.Constant
{
    class ConstantVariable
    {
        public static readonly string DATE_FORMAT_IN_WORD = "dd MMMM yyyy";
        public static readonly string DATE_FORMAT_DEFAULT = "yyyy-MM-dd";
        public static readonly string TIME_FORMAT_DEFAULT = "HH:mm:ss";

        public static readonly string LOCALE_REGION_INDONESIA = "id-ID";
        public static readonly string URL_PROTOCOL = "http://";

        public static readonly string DIR_PATH_CONFIG_FILE = @"\Configuration\config.json";
        public static readonly string DIR_PATH_CREATE_SETTLEMENT_FILE = @"/src/settlement.json";

        public static readonly string SETTLEMENT_UP_TO_DATE = "Up to Date : No Process Settlement Needed.";
        public static readonly string UPLOAD_JSON_FILE_SUCCESS = "JSON File has been uploaded successfully.";


        public static readonly string ERROR_MESSAGE_FAIL_TO_PARSE_FILE_CONFIG = "Error occurred while parsing configuration file.";
        public static readonly string ERROR_MESSAGE_CANNOT_ESTABLISH_CONNECTION_TO_SERVER = "Cannot connect to server.  Contact administrator.";
        public static readonly string ERROR_MESSAGE_INVALID_USERNAME_PASSWORD = "Invalid username/password, please try again.";
        public static readonly string ERROR_MESSAGE_UNABLE_TO_BACKUP = "Error , unable to backup!";
        public static readonly string ERROR_MESSAGE_UNABLE_TO_RESTORE = "Error , unable to Restore!";
        public static readonly string ERROR_MESSAGE_INSERT_SETTLEMENT_RECORD_INTO_DATABASE = "Error : something's wrong while inserting new record of settlement file.";
    }
}
