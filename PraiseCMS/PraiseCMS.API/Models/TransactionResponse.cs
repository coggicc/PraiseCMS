using System.Collections.Generic;

namespace PraiseCMS.API.Models
{
    public class TransactionResponse : ResultModel
    {
        public string account_number { get; set; }
        public string account_type { get; set; }
        public string approval_code { get; set; }
        public string authorization_code { get; set; }
        public string authorized_amount { get; set; }
        public string authorized_date { get; set; }
        public string avs_result_code { get; set; }
        public string avs_result_message { get; set; }
        public string balance_amount { get; set; }
        public string batch_id { get; set; }
        public string batch_number { get; set; }
        public string batch_sequence_number { get; set; }
        public string card_info_key { get; set; }
        public string card_key { get; set; }
        public string card_number_masked { get; set; }
        public string cash_back_amount { get; set; }
        public string check_key { get; set; }
        public string check_number { get; set; }
        public string check_type { get; set; }
        public string commercial_card { get; set; }
        public string contract_key { get; set; }
        public string customer_id { get; set; }
        public string customer_key { get; set; }
        public string customer_reference { get; set; }
        public string cv_result_code { get; set; }
        public string cv_result_message { get; set; }
        public string date { get; set; }
        public string date_of_birth { get; set; }
        public string driver_license { get; set; }
        public string email { get; set; }
        public string expiration_date { get; set; }
        public string gateway_id { get; set; }
        public string host_date { get; set; }
        public string host_reference_number { get; set; }
        public string host_time { get; set; }
        public string invoice_number { get; set; }
        public string ip_address { get; set; }
        public string last_update_date { get; set; }
        public string level3_amount { get; set; }
        public List<object> list_of_responses { get; set; }
        public string magnetic_ink_check_reader { get; set; }
        public string manual { get; set; }
        public string name_on_card { get; set; }
        public string name_on_check { get; set; }
        public string original_payment_reference_number { get; set; }
        public string payment_reference_number { get; set; }
        public string payment_type { get; set; }
        public string phone { get; set; }
        public string processor_id { get; set; }
        public string raw_magnetic_ink_check_reader { get; set; }
        public string receipt_count { get; set; }
        public string register_number { get; set; }
        public string reseller_id { get; set; }
        public string sub_result { get; set; }
        public string reversal_flag { get; set; }
        public string sequence_number { get; set; }
        public string settlement_date { get; set; }
        public string settlement_flag { get; set; }
        public string settlement_key { get; set; }
        public string settlement_message { get; set; }
        public string social_security_number { get; set; }
        public string state_code { get; set; }
        public string street1 { get; set; }
        public string street_match { get; set; }
        public string surcharge_amount { get; set; }
        public string tip_amount { get; set; }
        public string total_amount { get; set; }
        public string transaction_id { get; set; }
        public string transaction_type { get; set; }
        public string transit_number { get; set; }
        public string transport_endpoint { get; set; }
        public string transport_method { get; set; }
        public string type { get; set; }
        public string username { get; set; }
        public string void_flag { get; set; }
        public string zip_code { get; set; }
        public string zip_match { get; set; }
        public string last_ach_status_update_date { get; set; }
        public string card_type { get; set; }
        public string partial_reversal_flag { get; set; }
        public string requested_amount { get; set; }
        public string approved_amount { get; set; }
        public string recurring_id { get; set; }
        public string merchant_name { get; set; }
        public string benefit_expiration_date { get; set; }
        public List<EwicBalance> ewic_balance { get; set; }
        public string currency_code { get; set; }
        public string emv_data { get; set; }
        public Fragments fragments { get; set; }
        public CustomerToken customer_token { get; set; }
    }

    public class EwicBalance
    {
        public EwicBalanceInformationGroup ewic_balance_information_group { get; set; }
    }

    public class EwicBalanceInformationGroup
    {
        public string prescription_category_code { get; set; }
        public string prescription_sub_category_code { get; set; }
        public string prescription_quantity { get; set; }
    }

    public class Fragments
    {
        public List<Item> items { get; set; }
    }

    public class Item
    {
        public string merchant_key { get; set; }
        public int amount { get; set; }
        public string fragment_payment_reference_number { get; set; }
    }

    public class CustomerToken
    {
        public string customer_key { get; set; }
        public string card_info_key { get; set; }
        public string check_info_key { get; set; }
    }
}