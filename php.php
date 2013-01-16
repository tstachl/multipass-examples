<?php
$subdomain = 'YOUR DESK.COM SUBDOMAIN';
$api_key   = 'YOUR MULTIPASS API KEY';

// Create the encryption key using a 16 byte SHA1 digest of your api key and subdomain
$salted = $api_key . $subdomain;
$digest = hash('sha1', $salted, true);
$key    = substr($digest, 0, 16);

// Generate a random 16 byte IV
$iv = mcrypt_create_iv(16);

// Build json data
$user_data = array(
	'uid' => '123abc',
	'customer_email' => 'testuser@yoursite.com',
	'customer_name' => 'Test User',
	'expires' => date("c", strtotime("+5 minutes"))
);
$data = json_encode($user_data);

// PHP's mcrypt library does not perform padding by default
// Pad using standard PKCS#5 padding with block size of 16 bytes
$pad = 16 - (strlen($data) % 16);
$data = $data . str_repeat(chr($pad), $pad);

// Encrypt data using AES128-cbc
$cipher = mcrypt_module_open(MCRYPT_RIJNDAEL_128,'','cbc','');
mcrypt_generic_init($cipher, $key, $iv);
$multipass = mcrypt_generic($cipher,$data);
mcrypt_generic_deinit($cipher);

// Prepend the IV to the encrypted data
// This will be extracted and used for decryption
$multipass = $iv . $multipass;

// Base64 encode the encrypted data
$multipass = base64_encode($multipass);

// Build an HMAC-SHA1 signature using the encoded string and your api key
$signature = hash_hmac("sha1", $multipass, $api_key, true);
// Base64 encode the signature
$signature = base64_encode($signature);

// Finally, URL encode the multipass and signature
$multipass = urlencode($multipass);
$signature = urlencode($signature);

print "multipass: " . $multipass . "\n";
print "signature: " . $signature . "\n";
?>