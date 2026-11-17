import type { Metadata } from 'next';

export const metadata: Metadata = {
  // Template ở layout không áp cho trang cùng segment nên ghi rõ tên site.
  title: { absolute: 'Danh sách khóa học | TEDU Market' },
  description: 'Danh sách khóa học lập trình và công nghệ của TEDU Market.',
  alternates: { canonical: '/' },
};

/** `/` hiển thị Danh sách khóa học (ux.md mục 2). Lọc, sắp xếp, tìm kiếm, lưới thẻ khóa thuộc thẻ T47. */
export default function HomePage() {
  return (
    <section aria-labelledby="course-list-title">
      <h1 id="course-list-title" className="text-2xl font-bold md:text-3xl">
        Danh sách khóa học
      </h1>
      <p className="mt-4 text-text-muted">Chưa có khóa học nào</p>
    </section>
  );
}
