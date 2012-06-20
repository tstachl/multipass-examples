# You will need Python 2.6 or higher, and the following Python modules:
#   isodate
#   pycrypto

import json
import hashlib
import random
from datetime import datetime, timedelta
from base64 import b64encode

from isodate import datetime_isoformat
from Crypto.Cipher import AES
import hmac
import hashlib
import urllib

ACCOUNT_KEY = 'YOUR SITE KEY'
API_KEY = 'YOUR MULTIPASS API KEY'

def multipass_string(user_id, user_name, user_email):
	# Create the JSON for the multipass.
	multipass_json = json.dumps({
			'uid': user_id,
			'expires': datetime_isoformat(
							datetime.utcnow() + timedelta(minutes=2)),
			'customer_email': user_email,
			'customer_name': user_name
		})

	# Create the 16 byte salted hash.
	salt = API_KEY + ACCOUNT_KEY
	salted_hash = hashlib.sha1(salt).digest()[:16]

	# Seed the random number generator with the current time, then create a 
	# random Initialisation Vector (IV).
	random.seed()
	iv = ''.join(chr(random.randint(0, 0xFF)) for i in range(16))
	
	# Pad the JSON to a multiple of 16 bytes using PKCS#5 padding.
	padding_length = 16 - len(multipass_json) % 16
	multipass_json += ''.ljust(padding_length, chr(padding_length))

	# XOR the first 16-byte block with the IV.
	first_block = ''
	for i in range(16):
		first_block += chr(ord(multipass_json[i]) ^ ord(iv[i]))
	multipass_xored = first_block + multipass_json[16:]

	# Encrypt the multipass using AES encryption in CBC mode.
	encryptor = AES.new(salted_hash, AES.MODE_CBC, iv)
	multipass_encrypted = encryptor.encrypt(multipass_xored)

	# Encode the encrypted data using Base64 encoding
	return b64encode(multipass_encrypted)

def signature_string(multipass):
	signature = hmac.new(API_KEY, multipass, hashlib.sha1).digest()
	return b64encode(signature)

if __name__ == '__main__':
	multipass = multipass_string('0123457', 'Jan Anonymous', 'jan@anon.anon')
	signature = signature_string(multipass)

	# URL encode the multipass and signature parameters
	multipass = urllib.quote(multipass, '')
	signature = urllib.quote(signature, '')

	print "Multipass: %s\nSignature: %s" % (multipass, signature)
