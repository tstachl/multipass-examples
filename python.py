from Crypto import Random
from Crypto.Cipher import AES
from datetime import datetime, timedelta
from isodate import datetime_isoformat
import base64
import hashlib
import hmac
import urllib
import json
 
SITE_NAME     = 'your_desk_site_name'
MULTIPASS_KEY = 'your_multipass_key'
 
def pad(string, length):
  return string + (length - len(string) % length) * chr(length - len(string) % length) 
 
def multipass(json_data):
  salt  = hashlib.sha1(MULTIPASS_KEY + SITE_NAME).digest()[:16]
  iv    = Random.new().read(AES.block_size)
  aes   = AES.new(salt, AES.MODE_CBC, iv)
  multi = aes.encrypt(pad(json_data, AES.block_size))
  return base64.b64encode(iv + multi)
 
def signature(multipass):
  signature = hmac.new(MULTIPASS_KEY, multipass, hashlib.sha1).digest()
  return base64.b64encode(signature)
 
if __name__ == '__main__':
  mp  = multipass(json.dumps({
    'uid': '123',
    'customer_email': 'testuser@yoursite.com',
    'customer_name': 'Test User',
    'expires': datetime_isoformat(datetime.utcnow() + timedelta(minutes = 2))
    }))
  sg  = signature(mp)
  url = "https://%s.desk.com/customer/authentication/multipass/callback?multipass=%s&signature=%s"
  print url % (SITE_NAME, urllib.quote(mp), urllib.quote(sg))
