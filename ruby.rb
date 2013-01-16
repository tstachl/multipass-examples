require 'openssl'
require 'digest/sha1'
require 'base64'
require 'cgi'
require 'time'
require 'json'

SUBDOMAIN = "Your Desk.com Subdomain"
API_KEY = "Your Multipass API Key"

# Create the encryption key using a 16 byte SHA1 digest of your api key and subdomain
key = Digest::SHA1.digest(API_KEY + SUBDOMAIN)[0...16]

# Generate a random 16 byte IV
iv = OpenSSL::Random.random_bytes(16)

# Build the JSON string
json = JSON.generate(
  :uid => "123456",
  :expires => (Time.now + 120).iso8601, # Expire two minutes from now
  :customer_name => "John Doe",
  :customer_email => "john.doe@yoursite.com")

# Encrypt JSON string using AES128-CBC
cipher = OpenSSL::Cipher::Cipher.new("aes-128-cbc")
cipher.encrypt # specifies the cipher's mode (encryption vs decryption)
cipher.key = key
cipher.iv = iv
encrypted = cipher.update(json) + cipher.final

# Prepend encrypted data with the IV
prepended = iv + encrypted

# Base64 encode the prepended encrypted data
multipass = Base64.encode64(prepended)

# Build an HMAC-SHA1 signature using the encoded multipass and your api key
signature = Base64.encode64(OpenSSL::HMAC.digest('sha1', API_KEY, multipass))

# URL escape the final multipass and signature parameters
encoded_multipass = CGI.escape(multipass)
encoded_signature = CGI.escape(signature)

puts "multipass: #{encoded_multipass}"
puts "signature: #{encoded_signature}"