# Multipass example using the tender_multipass module
#  (https://bitbucket.org/mtrichardson/tender-multipass/)
# Install with pip: "pip install tender_multipass"

import base64
import datetime
import hashlib
import hmac
import pytz
import tender_multipass
import urllib

SITE_KEY = 'Your Site Key'
API_KEY = 'Your API Key'

expires = pytz.utc.localize(datetime.datetime.now() + 
                            datetime.timedelta(minutes=2))
data = {
    'customer_name': 'John Doe',
    'customer_email': 'john.doe@yoursite.com',
    'uid': '123456',
    'expires': expires.isoformat(),
}
multipass = tender_multipass.MultiPass(SITE_KEY, API_KEY)
multipass_string = multipass.encode(data)

signature = hmac.new(API_KEY, 
                     msg=multipass_string, 
                     digestmod=hashlib.sha1)
# urllib.quote() to mimic Ruby's CGI.escape
# http://docs.python.org/library/urllib.html#urllib.quote
signature_string = urllib.quote(base64.b64encode(signature.digest()))
