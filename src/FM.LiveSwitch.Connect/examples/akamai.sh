#!/bin/bash

timestamp="$(date +%s)"

# akamai
event="frozen2"

# liveswitch
applicationId="-- put your application id here --"
sharedSecret="-- put your shared secret here --"
channel="akamai-demo"

echo ""
echo -e "\033[32mJoin the session: \033[0m"
echo "https://demo.liveswitch.io/#gatewayurl=https://cloud.liveswitch.io&application=$applicationId&channel=$channel&mode=1&sharedsecret=$sharedSecret"
echo ""
echo -e "\033[32mView the session (delayed) via Akamai: \033[0m"
echo "http://players.akamai.com/players/hlsjs?streamUrl=https%3A%2F%2Ffrozen.akamaized.net%2Fcmaf%2Flive-ull%2F660746%2F$event%2Fmaster.m3u8"
echo ""

# run the project up one level
dotnet run -p ../ -- ffrender \
\
--gateway-url https://cloud.liveswitch.io/ \
--application-id $applicationId \
--shared-secret $sharedSecret \
--channel-id $channel \
--connection-id mcu \
--output-args="-flags +global_header -r 30000/1001 -filter_complex \"scale=1280x1024\" -pix_fmt yuv420p -c:v libx264 -b:v:0 3000K -maxrate:v:0 3000K -bufsize:v:0 3000K/2 -g:v 30 -keyint_min:v 30 -sc_threshold:v 0 -color_primaries bt709 -color_trc bt709 -colorspace bt709 -c:a aac -ar 48000 -ac 2 -b:a 96k -map 1:v:0 -map 0:a:0 -preset veryfast -tune zerolatency -hls_init_time 2.002 -hls_time 2.002 -hls_list_size 20 -hls_flags delete_segments -hls_base_url $timestamp/ -hls_segment_type fmp4 -var_stream_map \"a:0,agroup:a0,default:0 v:0,agroup:a0\" -hls_segment_filename http://p-ep660746.i.akamaientrypoint.net/cmaf/660746/$event/$timestamp/stream%v_%05d.m4s -master_pl_name master.m3u8 -http_user_agent Akamai_Broadcaster_v1.0 -http_persistent 1 -f hls http://p-ep660746.i.akamaientrypoint.net/cmaf/660746/$event/level_%v.m3u8"
