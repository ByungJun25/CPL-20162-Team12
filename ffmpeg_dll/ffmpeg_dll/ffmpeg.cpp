///> Include FFMpeg
extern "C" {
	#include <libswscale/swscale.h>
	#include <libavcodec/avcodec.h>
	#include <libavutil/frame.h>
	#include <libavformat/avformat.h>
	#include <libavutil/opt.h>
	#include <libavutil/imgutils.h>
	#include <libavutil/pixfmt.h>

}
#include <iostream>
///> Library Link On Windows System
#pragma comment( lib, "avformat.lib" )	
#pragma comment( lib, "avcodec.lib" )	
#pragma comment( lib, "avdevice.lib")
#pragma comment( lib, "avfilter.lib")
#pragma comment( lib, "avutil.lib")
#pragma comment( lib, "postproc.lib")
#pragma comment( lib, "swresample.lib")
#pragma comment( lib, "swscale.lib")

AVCodec *avcodec;
AVCodecContext *context;
int i, ret, x, y, got_output, outbuf_size, size;
AVFrame *frame, *raw_frame;
uint8_t *outbuf;

#define WIDTH 640
#define HEIGHT 480

typedef struct VideoPacket {
	uint8_t* data;
	int64_t pts;
	int64_t dts;
	int flags;
}VP;

extern "C" __declspec(dllexport)
int av_register_all_m(void)
{
	try{
		av_register_all();
	}
	catch(std::exception e){
		return 0;
	}
	return 1;
}
extern "C" __declspec(dllexport)
void avcodec_register_all_m(void)
{
	avcodec_register_all();
}
extern "C" __declspec(dllexport)
int ffmpeg_init(void)
{
	AVRational avRational;
	avRational.num = 1;
	avRational.den = 25;

	av_register_all();
	avcodec_register_all();

	avcodec = avcodec_find_encoder(AV_CODEC_ID_MPEG4);
	if (avcodec == NULL)
	{
		return 1;
	}
	context = avcodec_alloc_context3(avcodec);
	if (context == NULL)
	{
		return 2;
	}

	/* put sample parameters */
	context->codec_id = AV_CODEC_ID_MPEG4;
	context->rc_max_rate = 900 * 1000;
	context->bit_rate = context->rc_max_rate >> 1;
	context->rc_buffer_size = context->rc_max_rate;
	context->rc_initial_buffer_occupancy = context->rc_max_rate;

	/* resolution must be a multiple of two */
	context->width = WIDTH;
	context->height = HEIGHT;

	/* frames per second */
	context->time_base = avRational;
	context->gop_size = 25;
	context->pix_fmt = AV_PIX_FMT_YUV420P;



	/* open it */
	ret = avcodec_open2(context, avcodec, NULL);
	if (ret < 0)
	{
		return 3;
	}

	frame = av_frame_alloc();
	if (frame == NULL)
	{
		return 4;
	}

	frame->format = AV_PIX_FMT_YUV420P;
	frame->width = context->width;
	frame->height = context->height;

	raw_frame = av_frame_alloc();

	raw_frame->format = AV_PIX_FMT_NV21;
	raw_frame->width = context->width;
	raw_frame->height = context->height;


	outbuf_size = 640 * 480 * 3 / 2;
	outbuf = (uint8_t*)malloc(sizeof(uint8_t) * outbuf_size);
	memset(outbuf, 0, sizeof(uint8_t) * outbuf_size);

	
	return 0;
	
}

extern "C" __declspec(dllexport)
void avcodec_release(void)
{
	avcodec_close(context);
	av_free(context);
	av_freep(&frame->data[0]);
	av_freep(&raw_frame->data[0]);
	av_free(&frame);
	av_free(&raw_frame);
}



extern "C" __declspec(dllexport)
VP* encode_m(unsigned char* imbuffer)
{
	AVPacket pkt;
	VP* pkt_m;
	static struct SwsContext *swsContext;

	swsContext = sws_getContext(context->width, context->height, AV_PIX_FMT_NV21,
		context->width, context->height,
		AV_PIX_FMT_YUV420P,
		SWS_FAST_BILINEAR, NULL, NULL, NULL);

	av_init_packet(&pkt);
	pkt.data = NULL;    // packet data will be allocated by the encoder
	pkt.size = 0;

	
	// Fill raw_frame (NV21)
	avpicture_fill((AVPicture*)raw_frame, imbuffer,AV_PIX_FMT_NV21, raw_frame->width, raw_frame->height);

	// Fill dst frame (YUV420P)
	avpicture_fill((AVPicture*)frame, outbuf, AV_PIX_FMT_YUV420P, frame->width, frame->height);

	// Convert NV21 to YUV420P
	sws_scale(swsContext, raw_frame->data, raw_frame->linesize, 0, context->height, frame->data, frame->linesize);
	//       avpicture_fill((AVPicture*)frame, image, frame->format, frame->width, frame->height);

	frame->pts = AV_NOPTS_VALUE;

	ret = avcodec_encode_video2(context, &pkt, frame, &got_output);
	pkt_m = (VP *)malloc(sizeof(VP));
	memcpy(pkt_m->data, pkt.data, pkt.size);
	pkt_m->dts = pkt.dts;
	pkt_m->pts = pkt.pts;
	pkt_m->flags = pkt.flags;

	sws_freeContext(swsContext);
	
	return pkt_m;

}