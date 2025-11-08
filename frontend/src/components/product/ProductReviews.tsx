// src/components/product/ProductReviews.tsx
import React, { useState, useEffect, useCallback } from 'react';
import { Star, Send, User } from 'lucide-react';
import { getCookie } from '../../utils/cookies';
import { notify } from '../NotificationProvider';
import { format } from 'date-fns';

interface CommentDto {
  id: number;
  content: string;
  rating: number;
  createdAt: string;
  customerName: string;
  avatarUrl: string;
}

interface ProductReviewsProps {
  productId: number;
}

const ProductReviews: React.FC<ProductReviewsProps> = ({ productId }) => {
  const [comments, setComments] = useState<CommentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [nextLastCommentId, setNextLastCommentId] = useState<number | null>(null);

  const [newContent, setNewContent] = useState('');
  const [newRating, setNewRating] = useState(5);
  const [submitting, setSubmitting] = useState(false);

  // === FETCH COMMENTS ===
  const fetchComments = useCallback(
    async (lastCommentId?: number | null) => {
      if (lastCommentId === null) return;

      const isLoadMore = lastCommentId !== undefined;
      if (isLoadMore) setLoadingMore(true);
      else setLoading(true);

      try {
        const url = lastCommentId
          ? `http://localhost:5067/api/comments/product/${productId}?lastCommentId=${lastCommentId}&pageSize=5`
          : `http://localhost:5067/api/comments/product/${productId}`;

        const response = await fetch(url);
        const result = await response.json();

        if (response.ok && result.data) {
          const newComments: CommentDto[] = result.data.Comments || [];
          setHasMore(result.data.HasMore);
          setNextLastCommentId(result.data.NextLastCommentId);

          setComments((prev) =>
            isLoadMore ? [...prev, ...newComments] : newComments
          );
        } else {
          notify('error', result.message || 'Không thể tải đánh giá');
        }
      } catch (error) {
        notify('error', 'Không thể kết nối đến server');
        console.error('Fetch comments error:', error);
      } finally {
        setLoading(false);
        setLoadingMore(false);
      }
    },
    [productId]
  );

  // === SUBMIT REVIEW ===
  const handleSubmitReview = async () => {
    if (!newContent.trim()) {
      notify('warning', 'Vui lòng nhập nội dung đánh giá');
      return;
    }

    const token = getCookie('auth_token');
    if (!token) {
      notify('warning', 'Vui lòng đăng nhập để đánh giá');
      return;
    }

    setSubmitting(true);

    try {
      const response = await fetch('http://localhost:5067/api/comments', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify({
          content: newContent,
          rating: newRating,
          productId,
        }),
      });

      const result = await response.json();

      if (response.ok) {
        notify('success', result.message || 'Gửi đánh giá thành công');
        setNewContent('');
        setNewRating(5);
        // Tải lại 5 comment mới nhất
        await fetchComments();
      } else {
        notify('error', result.message || 'Gửi đánh giá thất bại');
      }
    } catch (error) {
      notify('error', 'Không thể kết nối đến server');
      console.error('Submit review error:', error);
    } finally {
      setSubmitting(false);
    }
  };

  // === LOAD MORE ===
  const handleLoadMore = () => {
    if (nextLastCommentId) {
      fetchComments(nextLastCommentId);
    }
  };

  // === INITIAL LOAD ===
  useEffect(() => {
    fetchComments();
  }, [fetchComments]);

  const formatDate = (dateString: string) => {
    try {
      return format(new Date(dateString), 'dd/MM/yyyy HH:mm');
    } catch {
      return 'Vừa xong';
    }
  };

  return (
    <div className="p-5 bg-white border shadow-xl sm:p-6 rounded-2xl border-violet-100/50">
      <h2 className="mb-5 text-lg font-bold text-gray-900 sm:text-xl">Đánh giá sản phẩm</h2>

      {/* === FORM GỬI ĐÁNH GIÁ === */}
      <div className="p-4 mb-5 border bg-violet-50 border-violet-200 rounded-2xl">
        <h3 className="mb-3 text-sm font-semibold text-gray-900">Viết đánh giá của bạn</h3>

        <div className="flex items-center gap-2 mb-3">
          <span className="text-sm font-medium text-gray-700">Đánh giá:</span>
          {[1, 2, 3, 4, 5].map((star) => (
            <button
              key={star}
              onClick={() => setNewRating(star)}
              className="focus:outline-none"
              disabled={submitting}
            >
              <Star
                className={`w-5 h-5 transition-colors ${
                  star <= newRating
                    ? 'fill-yellow-400 text-yellow-400'
                    : 'text-gray-300 hover:text-yellow-400'
                }`}
              />
            </button>
          ))}
        </div>

        <textarea
          value={newContent}
          onChange={(e) => setNewContent(e.target.value)}
          placeholder="Chia sẻ trải nghiệm của bạn về sản phẩm..."
          rows={3}
          disabled={submitting}
          className="w-full px-4 py-3 text-sm transition-all border border-gray-200 resize-none bg-gray-50 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white disabled:opacity-50 rounded-xl"
        />

        <button
          onClick={handleSubmitReview}
          disabled={submitting || !newContent.trim()}
          className="flex items-center gap-2 px-4 py-2 mt-3 text-sm font-semibold text-white transition-all bg-violet-800 hover:bg-violet-900 disabled:opacity-50 disabled:cursor-not-allowed rounded-xl"
        >
          <Send className="w-4 h-4" />
          {submitting ? 'Đang gửi...' : 'Gửi đánh giá'}
        </button>
      </div>

      {/* === DANH SÁCH COMMENT === */}
      <div className="space-y-4">
        {loading ? (
          <div className="flex items-center justify-center py-8">
            <div className="w-8 h-8 border-4 rounded-full border-violet-800 border-t-transparent animate-spin"></div>
          </div>
        ) : comments.length === 0 ? (
          <p className="py-6 text-sm text-center text-gray-500">Chưa có đánh giá nào</p>
        ) : (
          <>
            {comments.map((comment) => (
              <div
                key={comment.id}
                className="p-4 border border-gray-200 rounded-2xl"
              >
                <div className="flex items-start gap-3">
                  <div className="flex-shrink-0 w-10 h-10 overflow-hidden bg-gray-200 rounded-full">
                    {comment.avatarUrl ? (
                      <img
                        src={comment.avatarUrl}
                        alt={comment.customerName}
                        className="object-cover w-full h-full"
                      />
                    ) : (
                      <div className="flex items-center justify-center w-full h-full bg-gradient-to-br from-violet-600 to-violet-800">
                        <User className="w-5 h-5 text-white" />
                      </div>
                    )}
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center justify-between gap-2">
                      <h4 className="text-sm font-semibold text-gray-900 truncate">
                        {comment.customerName}
                      </h4>
                      <span className="flex-shrink-0 text-xs text-gray-500">
                        {formatDate(comment.createdAt)}
                      </span>
                    </div>
                    <div className="flex gap-1 mt-1">
                      {[...Array(5)].map((_, i) => (
                        <Star
                          key={i}
                          className={`w-3.5 h-3.5 ${
                            i < comment.rating ? 'fill-yellow-400 text-yellow-400' : 'text-gray-300'
                          }`}
                        />
                      ))}
                    </div>
                    <p className="mt-2 text-sm text-gray-700 break-words">{comment.content}</p>
                  </div>
                </div>
              </div>
            ))}

            {/* === NÚT LOAD MORE === */}
            {hasMore && (
              <button
                onClick={handleLoadMore}
                disabled={loadingMore}
                className="w-full py-3 mt-4 text-sm font-semibold transition-all bg-white border-2 shadow-sm text-violet-800 border-violet-200 hover:bg-violet-50 hover:shadow-md disabled:opacity-50 rounded-xl"
              >
                {loadingMore ? (
                  <span className="flex items-center justify-center gap-2">
                    <svg className="w-4 h-4 text-violet-800 animate-spin" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    Đang tải...
                  </span>
                ) : (
                  'Xem thêm đánh giá'
                )}
              </button>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default ProductReviews;