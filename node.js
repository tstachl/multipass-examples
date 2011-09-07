// This example courtesy of https://github.com/pinetechlabs
// https://gist.github.com/1173300

var qs = require('querystring'),
    crypto = require('crypto');

exports.sign = function (opt) {
    var data = new Buffer(JSON.stringify(opt.details), 'utf8'),
        iv = new Buffer('OpenSSL for Ruby', 'utf8'),
        i = 0,
        sha1 = crypto.createHash('sha1'),
        hash, cipher, ret;

    //add to the hash
    sha1.update(opt.apiKey + opt.siteKey);
    //slice it as only 16 bytes will be used for the key
    hash = new Buffer(sha1.digest(), 'binary').slice(0,16);
    //now do the iv xor thing
    for (i = 0; i < 16; i++)
        data[i] = data[i] ^ iv[i];

    //create a cipher
    cipher = crypto.createCipheriv('aes-128-cbc', hash.toString('binary'), iv.toString('binary'));
    //update the data
    ret = cipher.update(data.toString('utf8'), 'utf8', 'base64');
    ret += cipher.final('base64');


    return ret
        .replace(/=*$/, '')
        .replace(/\n/, '')
        .replace(/\+/, '-')
        .replace(/\//, '_');
};
/**
 * returns the multipass sso auth url for assitly
 * e.g.:
 * multipass({
 *     host: "help.yourcompany.com",
 *     details: {
 *         uid: "123",
 *         expires: '2011-08-30T00:00:00Z',
 *         customer_email: 'youruser@yourcompany.com',
 *         customer_name: 'Your User'
 *     },
 *     apiKey: '4e3b17bad70233ca20000004',
 *     siteKey: 'yourcompany'
 * });
 *
 *
 */

exports.multipass = function (opt) {
    var url =
        (opt.secure ? "https" : "http") +
        '://' +
        opt.host +
        '/customer/authentication/multipass/callback?multipass=' +
        qs.escape(exports.sign(opt));

    return url;
};