# Multipass example using entp's multipass gem (http://github.com/entp/multipass)
#   Make sure to install the gem: "gem install multipass"

require 'rubygems'
require 'multipass'
require 'openssl'
require 'cgi'
require 'time'

SITE_KEY = "Your Site Key"
API_KEY = "Your API Key"

multipass_string = MultiPass.encode(
  SITE_KEY,
  API_KEY,
  :customer_name => "John Doe",
  :customer_email => "john.doe@yoursite.com",
  :uid => "123456",
  :expires => (Time.now + 120).iso8601, # Expire two minutes from now
  :url_safe => true # Convert unsafe characters
)

signature = CGI.escape(Base64.encode64(OpenSSL::HMAC.digest('sha1', API_KEY, multipass_string)))