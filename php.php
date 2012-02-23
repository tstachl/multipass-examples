<?php
$account_key = 'YOUR SITE KEY';
$api_key     = 'YOUR MULTIPASS API KEY';

$salted = $api_key . $account_key;
$hash = hash('sha1',$salted,true);
$saltedHash = substr($hash,0,16);
$iv = "OpenSSL for Ruby";

// Build json data
$user_data = array(
	'uid' => '123abc',
	'customer_email' => 'testuser@yoursite.com',
	'customer_name' => 'Test User',
	'expires' => date("c", strtotime("+5 minutes"))
);
$data = json_encode($user_data);

// XOR first block of data with IV
for ($i = 0; $i < 16; $i++) {
	$data[$i] = $data[$i] ^ $iv[$i];
}

// pad using standard PKCS#5 padding with block size of 16 bytes
$pad = 16 - (strlen($data) % 16);
$data = $data . str_repeat(chr($pad), $pad);

// encrypt data using AES128-cbc
$cipher = mcrypt_module_open(MCRYPT_RIJNDAEL_128,'','cbc','');
mcrypt_generic_init($cipher, $saltedHash, $iv);
$multipass = mcrypt_generic($cipher,$data);
mcrypt_generic_deinit($cipher);

// Base64 encode the encrypted data
$multipass = base64_encode($multipass);

// Convert encoded data to the URL safe variant
$multipass = preg_replace('/\=$/', '', $multipass);
$multipass = preg_replace('/\n/', '', $multipass);
$multipass = preg_replace('/\+/', '-', $multipass);
$multipass = preg_replace('/\//', '_', $multipass);

// Build an HMAC-SHA1 signature using the multipass string and your API key
$signature = hash_hmac("sha1", $multipass, $api_key, true);
// Base64 encode the signature
$signature = base64_encode($signature);

// Finally, URL encode the multipass and signature
$multipass = urlencode($multipass);
$signature = urlencode($signature);
?>